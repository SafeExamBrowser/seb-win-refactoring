/*
 * Copyright (c) 2026 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 * Original code taken and adapted from https://github.com/eqsoft/seb2/blob/master/browser/app/modules/SebBrowser.jsm#L1215.
 */

/*
 * Implementation of the isolated clipboard (clipboard policy "SEB Only").
 *
 * The system clipboard is continuously cleared by the client, thus all clipboard content is kept by this script and shared with all other
 * frames and windows via the browser host (see Clipboard.cs and BrowserControl.cs).
 *
 * Copy, cut and paste events are intercepted in two stages:
 *
 *  1. A capturing listener on the window, which always runs first, prepares the event. For copy and cut, all data a web application writes
 *     via ClipboardEvent.clipboardData is recorded (and never reaches the system clipboard). For paste, ClipboardEvent.clipboardData is
 *     made to serve the content of the isolated clipboard.
 *  2. A bubbling listener, which is registered on the fly during the capturing phase and thus runs after all other listeners of the page,
 *     finalizes the event. If the web application handled the event itself (i.e. called preventDefault), its data is used. Otherwise, the
 *     default behavior of the browser is emulated and the native action is suppressed.
 *
 * This allows code editors (e.g. Monaco, Ace, CodeMirror) and rich text editors (e.g. TinyMCE, Atto, CKEditor) to keep using their own
 * clipboard handling, which fixes partial copies (e.g. only the first line of code) as well as duplicated pastes. Content is kept as plain
 * text and HTML, so that formatting is preserved within and across windows.
 */

if (typeof SafeExamBrowser.clipboard === 'undefined') {
	(function () {
		// Must match Clipboard.MaxContentLength in Clipboard.cs.
		var MAX_ENCODED_LENGTH = 16 * 1024 * 1024;
		var MAX_HTML_LENGTH = 4 * 1024 * 1024;
		var MIME_HTML = 'text/html';
		var MIME_TEXT = 'text/plain';
		var UNSAFE_ELEMENTS = 'script, style, link, meta, base, iframe, frame, frameset, object, embed, applet, noscript, template';
		var UNSAFE_URL = /^\s*(javascript|vbscript|data\s*:\s*text\/html)/i;

		var clipboard = SafeExamBrowser.clipboard = {
			id: generateId(),
			html: '',
			text: '',

			clear: function () {
				this.html = '';
				this.text = '';
			},

			getContentEncoded: function () {
				return encode(JSON.stringify({ html: this.html, text: this.text }));
			},

			update: function (id, base64) {
				if (this.id !== id) {
					try {
						var content = decode(base64);
						var data = parse(content);

						if (data) {
							this.html = data.html;
							this.text = data.text;
						} else {
							this.html = '';
							this.text = content;
						}
					} catch (e) {
						console.error('Failed to update clipboard content!', e);
					}
				}
			}
		};

		function generateId() {
			if (typeof crypto.randomUUID === 'function') {
				return crypto.randomUUID();
			}

			// crypto.randomUUID() is only available in secure contexts (i.e. not for plain HTTP pages).
			return Array.prototype.map.call(crypto.getRandomValues(new Uint8Array(16)), function (b) {
				return ('0' + b.toString(16)).slice(-2);
			}).join('');
		}

		function encode(value) {
			var bytes = new TextEncoder().encode(value);
			var binary = '';

			// Avoid passing huge arrays as arguments at once, as this would exceed the maximum call stack size for large content.
			for (var i = 0; i < bytes.length; i += 0x8000) {
				binary += String.fromCharCode.apply(null, bytes.subarray(i, i + 0x8000));
			}

			return btoa(binary);
		}

		function decode(base64) {
			var binary = atob(base64);
			var bytes = new Uint8Array(binary.length);

			for (var i = 0; i < binary.length; i++) {
				bytes[i] = binary.charCodeAt(i);
			}

			return new TextDecoder().decode(bytes);
		}

		function parse(content) {
			try {
				var data = JSON.parse(content);

				if (data && typeof data === 'object' && typeof data.text === 'string') {
					return { html: typeof data.html === 'string' ? data.html : '', text: data.text };
				}
			} catch (e) {
				// Not structured content, i.e. plain text.
			}

			return undefined;
		}

		function normalizeType(type) {
			var normalized = String(type).trim().toLowerCase();

			if (normalized === 'text' || normalized === 'text/unicode') {
				return MIME_TEXT;
			} else if (normalized === 'url') {
				return 'text/uri-list';
			}

			return normalized;
		}

		function define(target, name, descriptor) {
			descriptor.configurable = true;
			Object.defineProperty(target, name, descriptor);
		}

		function getTarget(e) {
			var path = typeof e.composedPath === 'function' ? e.composedPath() : [];

			// Use the innermost element to properly support input elements within shadow DOM trees.
			return path.length > 0 && path[0].nodeType === Node.ELEMENT_NODE ? path[0] : e.target;
		}

		function isTextControl(element) {
			if (element && (element.tagName === 'INPUT' || element.tagName === 'TEXTAREA')) {
				try {
					return typeof element.selectionStart === 'number';
				} catch (e) {
					return false;
				}
			}

			return false;
		}

		function isWritable(element) {
			if (isTextControl(element)) {
				return !element.readOnly && !element.disabled;
			}

			return !!(element && element.isContentEditable) || document.designMode === 'on';
		}

		function isPlainTextOnly(element) {
			var host = element && element.closest ? element.closest('[contenteditable]') : null;

			return !!host && String(host.getAttribute('contenteditable')).toLowerCase() === 'plaintext-only';
		}

		function htmlToText(html) {
			var template = document.createElement('template');

			template.innerHTML = html;

			return template.content.textContent || '';
		}

		function sanitize(html) {
			var template = document.createElement('template');

			template.innerHTML = html;
			template.content.querySelectorAll(UNSAFE_ELEMENTS).forEach(function (node) {
				node.remove();
			});
			template.content.querySelectorAll('*').forEach(function (node) {
				Array.prototype.slice.call(node.attributes).forEach(function (attribute) {
					var name = attribute.name.toLowerCase();
					var isEventHandler = name.indexOf('on') === 0;
					var isUnsafeUrl = /(^|:)(href|src|action|formaction)$/.test(name) && UNSAFE_URL.test(attribute.value);

					if (isEventHandler || isUnsafeUrl) {
						node.removeAttribute(attribute.name);
					}
				});
			});

			return template.innerHTML;
		}

		function notifyInput(target, inputType) {
			// Programmatic value changes (setRangeText / insertNode) emit no input event; notify frameworks (React, Vue, Angular) explicitly.
			if (target && typeof target.dispatchEvent === 'function') {
				target.dispatchEvent(new InputEvent('input', { bubbles: true, inputType: inputType }));
			}
		}

		function readSelection(target) {
			if (isTextControl(target)) {
				return { html: '', text: target.value.substring(target.selectionStart, target.selectionEnd) };
			}

			var selection = window.getSelection();
			var html = '';
			var text = selection ? selection.toString() : '';

			if (selection && selection.rangeCount > 0 && !selection.isCollapsed) {
				var container = document.createElement('div');

				for (var i = 0; i < selection.rangeCount; i++) {
					var range = selection.getRangeAt(i);
					var fragment = range.cloneContents();
					var ancestor = range.commonAncestorContainer;
					var element = ancestor.nodeType === Node.ELEMENT_NODE ? ancestor : ancestor.parentElement;
					var pre = element && element.closest ? element.closest('pre') : null;

					// Preserve the preformatting (i.e. whitespace and line breaks) when copying parts of a code block.
					if (pre && !fragment.querySelector('pre')) {
						var wrapper = document.createElement('pre');

						wrapper.appendChild(fragment);
						fragment = wrapper;
					}

					container.appendChild(fragment);
				}

				html = container.innerHTML;
			}

			return { html: html, text: text };
		}

		function readRecord(record) {
			var html = typeof record.data[MIME_HTML] === 'string' ? record.data[MIME_HTML] : '';
			var text = typeof record.data[MIME_TEXT] === 'string' ? record.data[MIME_TEXT] : '';

			if (!text && html) {
				text = htmlToText(html);
			}

			return { html: html, text: text };
		}

		function store(content) {
			clipboard.html = content.html && content.html.length <= MAX_HTML_LENGTH ? content.html : '';
			clipboard.text = content.text || '';

			publish();
		}

		function publish() {
			try {
				var encoded = clipboard.getContentEncoded();

				if (encoded.length > MAX_ENCODED_LENGTH && clipboard.html) {
					encoded = encode(JSON.stringify({ html: '', text: clipboard.text }));
				}

				if (encoded.length <= MAX_ENCODED_LENGTH) {
					CefSharp.PostMessage({ Type: 'Clipboard', Id: clipboard.id, Content: encoded });
				} else {
					console.warn('The clipboard content is too large to be shared with other frames and windows.');
				}
			} catch (e) {
				console.error('Failed to share clipboard content!', e);
			}
		}

		function recordWrites(dataTransfer, record) {
			define(dataTransfer, 'clearData', {
				value: function (type) {
					if (type === undefined) {
						Object.keys(record.data).forEach(function (key) {
							delete record.data[key];
						});
					} else {
						delete record.data[normalizeType(type)];
					}
				}
			});
			define(dataTransfer, 'getData', {
				value: function (type) {
					var value = record.data[normalizeType(type)];

					return typeof value === 'string' ? value : '';
				}
			});
			define(dataTransfer, 'setData', {
				value: function (type, value) {
					record.data[normalizeType(type)] = String(value);
					record.written = true;
				}
			});
			define(dataTransfer, 'types', {
				get: function () {
					return Object.freeze(Object.keys(record.data));
				}
			});
		}

		function provideContent(dataTransfer) {
			var data = {};

			if (clipboard.text) {
				data[MIME_TEXT] = clipboard.text;
			}

			if (clipboard.html) {
				data[MIME_HTML] = clipboard.html;
			}

			var types = Object.freeze(Object.keys(data));
			var items = types.map(function (type) {
				return {
					kind: 'string',
					type: type,
					getAsFile: function () {
						return null;
					},
					getAsString: function (callback) {
						if (typeof callback === 'function') {
							setTimeout(function () {
								callback(data[type]);
							}, 0);
						}
					},
					webkitGetAsEntry: function () {
						return null;
					}
				};
			});

			items.add = items.clear = items.remove = function () { };

			define(dataTransfer, 'files', { get: function () { return []; } });
			define(dataTransfer, 'getData', {
				value: function (type) {
					var value = data[normalizeType(type)];

					return typeof value === 'string' ? value : '';
				}
			});
			define(dataTransfer, 'items', { get: function () { return items; } });
			define(dataTransfer, 'types', { get: function () { return types; } });
		}

		function deleteSelection(target) {
			if (document.execCommand('delete', false)) {
				return;
			}

			if (isTextControl(target)) {
				target.setRangeText('', target.selectionStart, target.selectionEnd, 'end');
			} else {
				var selection = window.getSelection();

				for (var i = 0; i < selection.rangeCount; i++) {
					selection.getRangeAt(i).deleteContents();
				}
			}

			notifyInput(target, 'deleteByCut');
		}

		function insertContent(target, content) {
			if (isTextControl(target)) {
				if (content.text && !document.execCommand('insertText', false, content.text)) {
					target.setRangeText(content.text, target.selectionStart, target.selectionEnd, 'end');
					notifyInput(target, 'insertFromPaste');
				}
			} else {
				var useHtml = content.html && !isPlainTextOnly(target);
				var html = useHtml ? sanitize(content.html) : '';
				var success = false;

				if (useHtml) {
					success = document.execCommand('insertHTML', false, html);
				}

				if (!success && content.text) {
					success = document.execCommand('insertText', false, content.text);
				}

				if (!success && (html || content.text)) {
					insertManually(target, html, content.text);
				}
			}
		}

		function insertManually(target, html, text) {
			var selection = window.getSelection();

			if (selection && selection.rangeCount > 0) {
				var range = selection.getRangeAt(0);
				var node;

				if (html) {
					var template = document.createElement('template');

					template.innerHTML = html;
					node = document.importNode(template.content, true);
				} else {
					node = document.createTextNode(text);
				}

				var last = node.nodeType === Node.DOCUMENT_FRAGMENT_NODE ? node.lastChild : node;

				range.deleteContents();
				range.insertNode(node);

				if (last) {
					range.setStartAfter(last);
					range.collapse(true);
					selection.removeAllRanges();
					selection.addRange(range);
				}

				notifyInput(target, 'insertFromPaste');
			}
		}

		function completeCopy(state, dispatching) {
			var content;

			if (state.event.defaultPrevented) {
				// The web application has handled the event itself, thus only the data it provided is relevant. If it did not provide any
				// data, it deliberately prevented copying and the content of the clipboard must remain unchanged.
				content = state.record.written ? readRecord(state.record) : undefined;
			} else {
				content = dispatching ? readSelection(state.target) : state.snapshot;
			}

			if (content && (content.text || content.html)) {
				store(content);
			}
		}

		function completeCut(state, dispatching) {
			if (state.event.defaultPrevented) {
				// The web application has handled the event (including the removal of the selected content) itself.
				if (state.record.written) {
					store(readRecord(state.record));
				}
			} else if (dispatching) {
				var content = readSelection(state.target);

				if (content.text || content.html) {
					store(content);

					if (isWritable(state.target)) {
						deleteSelection(state.target);
					}
				}
			} else if (state.snapshot && (state.snapshot.text || state.snapshot.html)) {
				// The native action has already been executed, i.e. the selected content has already been removed.
				store(state.snapshot);
			}
		}

		function completePaste(state) {
			// If the web application has handled the event itself, it did so using the content provided via ClipboardEvent.clipboardData.
			if (!state.event.defaultPrevented && isWritable(state.target)) {
				insertContent(state.target, { html: clipboard.html, text: clipboard.text });
			}
		}

		function complete(state, dispatching) {
			if (state.completed) {
				return;
			}

			state.completed = true;
			window.removeEventListener(state.event.type, state.listener, false);

			try {
				if (state.event.type === 'copy') {
					completeCopy(state, dispatching);
				} else if (state.event.type === 'cut') {
					completeCut(state, dispatching);
				} else if (state.event.type === 'paste') {
					completePaste(state);
				}
			} catch (e) {
				console.error('Failed to process ' + state.event.type + ' event!', e);
			} finally {
				if (dispatching) {
					// Never let the browser access the system clipboard.
					state.event.preventDefault();
				}
			}
		}

		function onClipboardEvent(e) {
			// Synthetic events created by web applications do not interact with the system clipboard and are thus left untouched.
			if (!e.isTrusted) {
				return;
			}

			var state = { completed: false, event: e, record: { data: {}, written: false }, target: getTarget(e) };

			try {
				if (e.type === 'paste') {
					if (e.clipboardData) {
						provideContent(e.clipboardData);
					}
				} else {
					state.snapshot = readSelection(state.target);

					if (e.clipboardData) {
						recordWrites(e.clipboardData, state.record);
					}
				}
			} catch (error) {
				console.error('Failed to prepare ' + e.type + ' event!', error);
			}

			state.listener = function (event) {
				if (event === state.event) {
					complete(state, true);
				}
			};

			// This listener is registered during the capturing phase and will thus be invoked as last listener of the bubbling phase.
			window.addEventListener(e.type, state.listener, false);

			// Fallback in case the web application stopped the propagation of the event.
			setTimeout(function () {
				complete(state, false);
			}, 0);
		}

		function initializeAsyncClipboard() {
			var api = navigator.clipboard;

			if (!api) {
				return;
			}

			var ensureFocus = function () {
				if (!document.hasFocus()) {
					throw new DOMException('Document is not focused.', 'NotAllowedError');
				}
			};

			// Route the asynchronous clipboard API to the isolated clipboard, as the system clipboard is not available.
			define(api, 'readText', {
				value: function () {
					return Promise.resolve().then(function () {
						ensureFocus();

						return clipboard.text;
					});
				}
			});
			define(api, 'writeText', {
				value: function (text) {
					return Promise.resolve().then(function () {
						ensureFocus();
						store({ html: '', text: String(text) });
					});
				}
			});

			if (typeof ClipboardItem === 'function') {
				define(api, 'read', {
					value: function () {
						return Promise.resolve().then(function () {
							var data = {};

							ensureFocus();

							if (clipboard.text) {
								data[MIME_TEXT] = new Blob([clipboard.text], { type: MIME_TEXT });
							}

							if (clipboard.html) {
								data[MIME_HTML] = new Blob([clipboard.html], { type: MIME_HTML });
							}

							return Object.keys(data).length > 0 ? [new ClipboardItem(data)] : [];
						});
					}
				});
				define(api, 'write', {
					value: function (items) {
						return Promise.resolve().then(function () {
							var content = { html: '', text: '' };
							var reads = [];

							ensureFocus();

							Array.prototype.forEach.call(items || [], function (item) {
								[MIME_HTML, MIME_TEXT].forEach(function (type) {
									if (item.types.indexOf(type) >= 0) {
										reads.push(item.getType(type).then(function (blob) {
											return blob.text();
										}).then(function (value) {
											content[type === MIME_HTML ? 'html' : 'text'] = value;
										}));
									}
								});
							});

							return Promise.all(reads).then(function () {
								if (!content.text && content.html) {
									content.text = htmlToText(content.html);
								}

								store(content);
							});
						});
					}
				});
			}
		}

		try {
			initializeAsyncClipboard();
		} catch (e) {
			console.error('Failed to initialize asynchronous clipboard API!', e);
		}

		window.addEventListener('copy', onClipboardEvent, true);
		window.addEventListener('cut', onClipboardEvent, true);
		window.addEventListener('paste', onClipboardEvent, true);
	})();
}
