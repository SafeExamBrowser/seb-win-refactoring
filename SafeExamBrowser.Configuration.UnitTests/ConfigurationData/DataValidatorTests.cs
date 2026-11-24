/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SafeExamBrowser.Configuration.ConfigurationData;
using SafeExamBrowser.Logging.Contracts;

namespace SafeExamBrowser.Configuration.UnitTests.ConfigurationData
{
	[TestClass]
	public class DataValidatorTests
	{
		private const string HASH = "5e884898da28047151d0e56f8dc6292773603d0d6aabbdd62a11ef721d1542d8";
		private const string INVALID_SEQUENCE = "\",";

		private Mock<ILogger> logger;
		private DataValidator sut;

		[TestInitialize]
		public void Initialize()
		{
			logger = new Mock<ILogger>();
			sut = new DataValidator(logger.Object);
		}

		[TestMethod]
		public void MustAcceptEmptyData()
		{
			var valid = sut.Validate(new Dictionary<string, object>());

			logger.Verify(l => l.Debug(It.IsAny<string>()), Times.Once);
			logger.Verify(l => l.Warn(It.IsAny<string>()), Times.Never);
			logger.Verify(l => l.Error(It.IsAny<string>()), Times.Never);

			Assert.IsTrue(valid);
		}

		[TestMethod]
		public void MustAcceptValidData()
		{
			var data = new Dictionary<string, object>
			{
				{ Keys.Security.AdminPasswordHash, HASH },
				{ Keys.Security.QuitPasswordHash, HASH.ToUpperInvariant() },
				{ Keys.Server.FallbackPasswordHash, HASH.Substring(0, 32) + HASH.Substring(32).ToUpperInvariant() },
				{ "someString", "A \"quoted\" value, followed by a comma" },
				{ "someNumber", 42 },
				{ "someBoolean", true },
				{ "someData", Encoding.UTF8.GetBytes(INVALID_SEQUENCE) },
				{ "someDictionary", new Dictionary<string, object> { { "nested", "a, b, c" } } },
				{ "someArray", new List<object> { "x", 1, new Dictionary<string, object> { { "nested", ",\"" } } } }
			};

			var valid = sut.Validate(data);

			logger.Verify(l => l.Debug(It.IsAny<string>()), Times.Once);
			logger.Verify(l => l.Warn(It.IsAny<string>()), Times.Never);
			logger.Verify(l => l.Error(It.IsAny<string>()), Times.Never);

			Assert.IsTrue(valid);
		}

		[TestMethod]
		public void MustAcceptEmptyPasswordHashes()
		{
			var data = new Dictionary<string, object>
			{
				{ Keys.Security.AdminPasswordHash, string.Empty },
				{ Keys.Security.QuitPasswordHash, string.Empty },
				{ Keys.Server.FallbackPasswordHash, string.Empty }
			};

			var valid = sut.Validate(data);

			logger.Verify(l => l.Warn(It.IsAny<string>()), Times.Never);
			Assert.IsTrue(valid);
		}

		[TestMethod]
		public void MustAcceptNullPasswordHashes()
		{
			var data = new Dictionary<string, object>
			{
				{ Keys.Security.AdminPasswordHash, null },
				{ Keys.Security.QuitPasswordHash, null },
				{ Keys.Server.FallbackPasswordHash, null }
			};

			var valid = sut.Validate(data);

			logger.Verify(l => l.Warn(It.IsAny<string>()), Times.Never);
			Assert.IsTrue(valid);
		}

		[TestMethod]
		public void MustRejectMalformedPasswordHashes()
		{
			var keys = new[]
			{
				Keys.Security.AdminPasswordHash,
				Keys.Security.QuitPasswordHash,
				Keys.Server.FallbackPasswordHash
			};
			var values = new[]
			{
				HASH.Substring(1),
				HASH + "0",
				"g" + HASH.Substring(1),
				" " + HASH,
				HASH + " ",
				" ",
				"password"
			};

			foreach (var key in keys)
			{
				foreach (var value in values)
				{
					var data = new Dictionary<string, object> { { key, value } };
					var valid = sut.Validate(data);

					Assert.IsFalse(valid, $"The value '{value}' for '{key}' must be rejected.");
				}

				logger.Verify(l => l.Warn(It.Is<string>(m => m.Contains(key))), Times.Exactly(values.Length));
				logger.Verify(l => l.Error(It.IsAny<string>()), Times.Exactly(values.Length));
				logger.Verify(l => l.Debug(It.IsAny<string>()), Times.Never);

				logger.Reset();
			}
		}

		[TestMethod]
		public void MustRejectPasswordHashesOfWrongType()
		{
			var keys = new[]
			{
				Keys.Security.AdminPasswordHash,
				Keys.Security.QuitPasswordHash,
				Keys.Server.FallbackPasswordHash
			};
			var values = new object[]
			{
				12345,
				true,
				Encoding.ASCII.GetBytes(HASH),
				new List<object> { HASH },
				new Dictionary<string, object>()
			};

			foreach (var key in keys)
			{
				foreach (var value in values)
				{
					var data = new Dictionary<string, object> { { key, value } };
					var valid = sut.Validate(data);

					Assert.IsFalse(valid, $"The value of type '{value.GetType()}' for '{key}' must be rejected.");
				}

				logger.Verify(l => l.Warn(It.Is<string>(m => m.Contains(key))), Times.Exactly(values.Length));
				logger.Verify(l => l.Error(It.IsAny<string>()), Times.Exactly(values.Length));

				logger.Reset();
			}
		}

		[TestMethod]
		public void MustRejectValuesWithInvalidCharacterSequence()
		{
			var values = new[]
			{
				"\",",
				"\" ,",
				"\"\t,",
				"\"\r\n,",
				"A \"quoted\", value"
			};

			foreach (var value in values)
			{
				var data = new Dictionary<string, object> { { "someKey", value } };
				var valid = sut.Validate(data);

				logger.Verify(l => l.Warn(It.Is<string>(m => m.Contains("someKey"))), Times.Once);
				logger.Verify(l => l.Error(It.IsAny<string>()), Times.Once);
				logger.Verify(l => l.Debug(It.IsAny<string>()), Times.Never);

				logger.Reset();

				Assert.IsFalse(valid);
			}
		}

		[TestMethod]
		public void MustAcceptValuesWithoutInvalidCharacterSequence()
		{
			var values = new[]
			{
				"",
				"\"",
				",",
				",\"",
				"\"x,",
				"\"\" x,"
			};

			foreach (var value in values)
			{
				var data = new Dictionary<string, object> { { "someKey", value } };
				var valid = sut.Validate(data);

				logger.Verify(l => l.Warn(It.IsAny<string>()), Times.Never);
				logger.Reset();

				Assert.IsTrue(valid);
			}
		}

		[TestMethod]
		public void MustRejectKeysWithInvalidCharacterSequence()
		{
			var key = "some" + INVALID_SEQUENCE + "Key";
			var data = new Dictionary<string, object> { { key, 1 } };

			var valid = sut.Validate(data);

			logger.Verify(l => l.Warn(It.Is<string>(m => m.Contains(key))), Times.Once);
			logger.Verify(l => l.Error(It.IsAny<string>()), Times.Once);

			Assert.IsFalse(valid);
		}

		[TestMethod]
		public void MustValidateNestedDictionaries()
		{
			var data = new Dictionary<string, object>
			{
				{
					"outer", new Dictionary<string, object>
					{
						{ "valid", "valid" },
						{ "inner", new Dictionary<string, object> { { "innermost", INVALID_SEQUENCE } } }
					}
				}
			};

			var valid = sut.Validate(data);

			logger.Verify(l => l.Warn(It.Is<string>(m => m.Contains("innermost"))), Times.Once);
			logger.Verify(l => l.Warn(It.IsAny<string>()), Times.Once);

			Assert.IsFalse(valid);
		}

		[TestMethod]
		public void MustValidateKeysOfNestedDictionaries()
		{
			var key = "inner" + INVALID_SEQUENCE;
			var data = new Dictionary<string, object>
			{
				{ "outer", new Dictionary<string, object> { { key, true } } }
			};

			var valid = sut.Validate(data);

			logger.Verify(l => l.Warn(It.Is<string>(m => m.Contains(key))), Times.Once);
			Assert.IsFalse(valid);
		}

		[TestMethod]
		public void MustValidateArrayElements()
		{
			var data = new Dictionary<string, object>
			{
				{ "someArray", new List<object> { "valid", 1, INVALID_SEQUENCE } }
			};

			var valid = sut.Validate(data);

			logger.Verify(l => l.Warn(It.Is<string>(m => m.Contains("someArray"))), Times.Once);
			Assert.IsFalse(valid);
		}

		[TestMethod]
		public void MustValidateNestedArrays()
		{
			var data = new Dictionary<string, object>
			{
				{ "someArray", new List<object> { new List<object> { new List<object> { INVALID_SEQUENCE } } } }
			};

			var valid = sut.Validate(data);

			logger.Verify(l => l.Warn(It.Is<string>(m => m.Contains("someArray"))), Times.Once);
			Assert.IsFalse(valid);
		}

		[TestMethod]
		public void MustValidateDictionariesWithinArrays()
		{
			var data = new Dictionary<string, object>
			{
				{
					"rules", new List<object>
					{
						new Dictionary<string, object> { { "expression", "*.valid.org" }, { "active", true } },
						new Dictionary<string, object> { { "expression", "invalid" + INVALID_SEQUENCE }, { "active", true } }
					}
				}
			};

			var valid = sut.Validate(data);

			logger.Verify(l => l.Warn(It.Is<string>(m => m.Contains("expression"))), Times.Once);
			logger.Verify(l => l.Warn(It.IsAny<string>()), Times.Once);

			Assert.IsFalse(valid);
		}

		[TestMethod]
		public void MustReportAllViolations()
		{
			var data = new Dictionary<string, object>
			{
				{ Keys.Security.AdminPasswordHash, "invalid" },
				{ Keys.Security.QuitPasswordHash, 1 },
				{ Keys.Server.FallbackPasswordHash, HASH + "0" },
				{ "invalid" + INVALID_SEQUENCE + "Key", "valid" },
				{ "someDictionary", new Dictionary<string, object> { { "invalidValue", INVALID_SEQUENCE } } }
			};

			var valid = sut.Validate(data);

			logger.Verify(l => l.Warn(It.Is<string>(m => m.Contains(Keys.Security.AdminPasswordHash))), Times.Once);
			logger.Verify(l => l.Warn(It.Is<string>(m => m.Contains(Keys.Security.QuitPasswordHash))), Times.Once);
			logger.Verify(l => l.Warn(It.Is<string>(m => m.Contains(Keys.Server.FallbackPasswordHash))), Times.Once);
			logger.Verify(l => l.Warn(It.Is<string>(m => m.Contains("invalid" + INVALID_SEQUENCE + "Key"))), Times.Once);
			logger.Verify(l => l.Warn(It.Is<string>(m => m.Contains("invalidValue"))), Times.Once);
			logger.Verify(l => l.Warn(It.IsAny<string>()), Times.Exactly(5));
			logger.Verify(l => l.Error(It.IsAny<string>()), Times.Once);
			logger.Verify(l => l.Debug(It.IsAny<string>()), Times.Never);

			Assert.IsFalse(valid);
		}
	}
}
