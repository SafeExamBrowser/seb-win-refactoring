using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using SebWindowsConfig.Entities;
using SebWindowsConfig.Utilities;
using DictObj = System.Collections.Generic.Dictionary<string, object>;
using ListObj = System.Collections.Generic.List<object>;

namespace SebWindowsConfig.Controls
{
	public partial class AdditionalResources : UserControl
	{
		private IFileCompressor _fileCompressor;
		private SEBURLFilter urlFilter;


		public AdditionalResources()
		{
			InitializeComponent();

			_fileCompressor = new FileCompressor();

			groupBoxAdditionalResourceDetails.Visible = false;

			SEBSettings.additionalResourcesList = (ListObj)SEBSettings.settingsCurrent[SEBSettings.KeyAdditionalResources];
			textBoxAdditionalResourcesTitle.Text = "";
			treeViewAdditionalResources.Nodes.Clear();
			foreach (DictObj l0Resource in SEBSettings.additionalResourcesList)
			{
				var l0Node = treeViewAdditionalResources.Nodes.Add(l0Resource[SEBSettings.KeyAdditionalResourcesIdentifier].ToString(), GetDisplayTitle(l0Resource));
				foreach (DictObj l1Resource in (ListObj)l0Resource[SEBSettings.KeyAdditionalResources])
				{
					var l1Node = l0Node.Nodes.Add(l1Resource[SEBSettings.KeyAdditionalResourcesIdentifier].ToString(), GetDisplayTitle(l1Resource));
					foreach (DictObj l2Resource in (ListObj)l1Resource[SEBSettings.KeyAdditionalResources])
					{
						l1Node.Nodes.Add(l2Resource[SEBSettings.KeyAdditionalResourcesIdentifier].ToString(), GetDisplayTitle(l2Resource));
					}
				}
			}

			urlFilter = new SEBURLFilter();
		}

		private string GetDisplayTitle(DictObj resource)
		{
			return string.Concat( 
				resource[SEBSettings.KeyAdditionalResourcesTitle],
				(bool) resource[SEBSettings.KeyAdditionalResourcesActive] ? "" : " (inactive)",
				(bool) resource[SEBSettings.KeyAdditionalResourcesAutoOpen] ? " (A)" : "",
				resource.ContainsKey(SEBSettings.KeyAdditionalResourcesShowButton) ? (bool) resource[SEBSettings.KeyAdditionalResourcesShowButton] ? " (B)" : "" : "",
				!string.IsNullOrEmpty((string)resource[SEBSettings.KeyAdditionalResourcesResourceData]) ? " (E)" : "",
				!string.IsNullOrEmpty((string)resource[SEBSettings.KeyAdditionalResourcesUrl]) ? " (U)" : "");
		}

		private void buttonAdditionalResourcesAdd_Click(object sender, EventArgs e)
		{
			// Get the process list
			SEBSettings.additionalResourcesList = (ListObj)SEBSettings.settingsCurrent[SEBSettings.KeyAdditionalResources];

			int newIndex = treeViewAdditionalResources.Nodes.Count;
			SEBSettings.additionalResourcesList.Insert(newIndex, CreateNewResource(newIndex.ToString()));

			treeViewAdditionalResources.SelectedNode = treeViewAdditionalResources.Nodes.Add(newIndex.ToString(), "New Resource");
			treeViewAdditionalResources.Focus();
		}

		private DictObj CreateNewResource(string identifier)
		{
			DictObj resourceData = new DictObj();
			resourceData[SEBSettings.KeyAdditionalResourcesIdentifier] = identifier;
			return SetDefaultValuesOnResource(resourceData);
		}

		private void buttonAdditionalResourcesAddSubResource_Click(object sender, EventArgs e)
		{
			var node = treeViewAdditionalResources.SelectedNode;
			if (node == null)
			{
				MessageBox.Show("No node selected");
				return;
			}
			if (node.Level == 2)
			{
				MessageBox.Show("Maximum 3 levels");
				return;
			}

			var selectedResource = GetSelectedResource();
			ListObj resourceList = (ListObj)selectedResource[SEBSettings.KeyAdditionalResources];

			var newIndex = node.Nodes.Count;
			if (node.Level == 0)
			{
				resourceList.Add(CreateNewResource(node.Index + "." + newIndex));
				treeViewAdditionalResources.SelectedNode = treeViewAdditionalResources.SelectedNode.Nodes.Add(node.Index + "." + newIndex, "New Resource");
			}
			if (node.Level == 1)
			{
				resourceList.Add(CreateNewResource(node.Parent.Index + "." + node.Index + "." + newIndex));
				treeViewAdditionalResources.SelectedNode = treeViewAdditionalResources.SelectedNode.Nodes.Add(node.Parent.Index + "." + node.Index + "." + newIndex, "New Resource");
			}
			treeViewAdditionalResources.Focus();
		}

		private void treeViewAdditionalResources_AfterSelect(object sender, TreeViewEventArgs e)
		{
			DictObj selectedResource = GetSelectedResource();
			if (selectedResource != null)
			{
				textBoxAdditionalResourcesTitle.Text = (string)selectedResource[SEBSettings.KeyAdditionalResourcesTitle];
				checkBoxAdditionalResourceActive.Checked = (bool)selectedResource[SEBSettings.KeyAdditionalResourcesActive];
				textBoxAdditionalResourceUrl.Text = (string)selectedResource[SEBSettings.KeyAdditionalResourcesUrl];
				checkBoxAdditionalResourceAutoOpen.Checked = (bool)selectedResource[SEBSettings.KeyAdditionalResourcesAutoOpen];
				textBoxLinkURL.Text = (string)selectedResource[SEBSettings.KeyAdditionalResourcesLinkUrl];
				textBoxRefererFilter.Text = (string) selectedResource[SEBSettings.KeyAdditionalResourcesRefererFilter];
				checkBoxResetSession.Checked = (bool) selectedResource[SEBSettings.KeyAdditionalResourcesResetSession];
				textBoxKey.Text = (string) selectedResource[SEBSettings.KeyAdditionalResourcesKey];
				checkBoxConfirm.Checked = (bool)selectedResource[SEBSettings.KeyAdditionalResourcesConfirm];
				textBoxConfirmBoxText.Text = (string) selectedResource[SEBSettings.KeyAdditionalResourcesConfirmText];
				checkBoxShowButton.Checked = (bool) selectedResource[SEBSettings.KeyAdditionalResourcesShowButton];

				comboBoxModifiers.SelectedItem = string.IsNullOrEmpty((string) selectedResource[SEBSettings.KeyAdditionalResourcesModifiers]) ? null : (string)selectedResource[SEBSettings.KeyAdditionalResourcesModifiers];

				InitializeUrlFilters(selectedResource);

				if (!string.IsNullOrEmpty((string)selectedResource[SEBSettings.KeyAdditionalResourcesResourceData]))
				{
					buttonAdditionalResourceRemoveResourceData.Visible = true;
					buttonAdditionalResourceEmbededResourceOpen.Visible = true;
					labelAdditionalResourcesResourceDataLaunchWith.Visible = true;
					labelAdditionalResourcesFilename.Visible = true;
					comboBoxAdditionalResourcesResourceDataLauncher.Visible = true;
					textBoxAdditionalResourceUrl.Enabled = false;
					comboBoxAdditionalResourcesChooseFileToLaunch.Visible = true;
					comboBoxAdditionalResourcesChooseFileToLaunch.Enabled = false;

					var indexBefore = (int) selectedResource[SEBSettings.KeyAdditionalResourcesResourceDataLauncher];
					comboBoxAdditionalResourcesResourceDataLauncher.DataSource = GetLaunchers();
					comboBoxAdditionalResourcesResourceDataLauncher.SelectedIndex = indexBefore;
					comboBoxAdditionalResourcesChooseFileToLaunch.Text =
						selectedResource[SEBSettings.KeyAdditionalResourcesResourceDataFilename].ToString();
				}
				else
				{
					buttonAdditionalResourceRemoveResourceData.Visible = false;
					buttonAdditionalResourceEmbededResourceOpen.Visible = false;
					labelAdditionalResourcesResourceDataLaunchWith.Visible = false;
					labelAdditionalResourcesFilename.Visible = false;
					comboBoxAdditionalResourcesResourceDataLauncher.Visible = false;
					textBoxAdditionalResourceUrl.Enabled = true;
					comboBoxAdditionalResourcesChooseFileToLaunch.Visible = false;
				}

				if (!string.IsNullOrEmpty((string) selectedResource[SEBSettings.KeyAdditionalResourcesUrl]))
				{
					buttonAdditionalResourceChooseEmbededResource.Enabled = false;
				}
				else
				{
					buttonAdditionalResourceChooseEmbededResource.Enabled = true;
				}

				if (((ListObj) selectedResource[SEBSettings.KeyAdditionalResourcesResourceIcons]).Count > 0)
				{
					var icon =
						(DictObj) ((ListObj) selectedResource[SEBSettings.KeyAdditionalResourcesResourceIcons])[0];
					var memoryStream =
							_fileCompressor.DeCompressAndDecode(
								(string) icon[SEBSettings.KeyAdditionalResourcesResourceIconsIconData]);
					var image = Image.FromStream(memoryStream);
					pictureBoxAdditionalResourceIcon.Image = image;
				}
				else
				{
					pictureBoxAdditionalResourceIcon.Image = null;
				}
				
			}
			groupBoxAdditionalResourceDetails.Visible = selectedResource != null;
		}

		public void InitializeUrlFilters(DictObj resourceConfig)
		{
			var filterControl = new FilterRuleControl();

			if (resourceConfig.ContainsKey(SEBSettings.KeyURLFilterRules))
			{
				foreach (DictObj config in resourceConfig[SEBSettings.KeyURLFilterRules] as ListObj)
				{
					var rule = FilterRule.FromConfig(config);

					filterControl.AddRule(rule);
				}
			}

			filterControl.Dock = DockStyle.Fill;
			filterControl.DataChanged += (rules) =>
			{
				var configs = new ListObj();

				foreach (var rule in rules)
				{
					var config = FilterRule.ToConfig(rule);

					configs.Add(config);
				}

				resourceConfig[SEBSettings.KeyURLFilterRules] = configs;
			};

			UrlFilterGroupBox.Controls.Clear();
			UrlFilterGroupBox.Controls.Add(filterControl);
		}

		private List<string> GetLaunchers()
		{
			var res = new List<string>();
			foreach (DictObj permittedProcess in SEBSettings.permittedProcessList)
			{
				res.Add((string)permittedProcess[SEBSettings.KeyTitle]);
			}
			return res;
		}

		private DictObj GetSelectedResource()
		{
			var node = treeViewAdditionalResources.SelectedNode;

			if (node.Level == 0)
			{
				return SetDefaultValuesOnResource((DictObj)SEBSettings.additionalResourcesList[node.Index]);
			}
			else if (node.Level == 1)
			{
				DictObj rootResource = (DictObj)SEBSettings.additionalResourcesList[node.Parent.Index];
				ListObj level1List = (ListObj)rootResource[SEBSettings.KeyAdditionalResources];
				return SetDefaultValuesOnResource((DictObj)level1List[node.Index]);
			}
			else if (node.Level == 2)
			{
				DictObj rootResource = (DictObj)SEBSettings.additionalResourcesList[treeViewAdditionalResources.SelectedNode.Parent.Parent.Index];
				ListObj level1List = (ListObj)rootResource[SEBSettings.KeyAdditionalResources];
				DictObj level1Resource = (DictObj)level1List[treeViewAdditionalResources.SelectedNode.Parent.Index];
				ListObj level2List = (ListObj)level1Resource[SEBSettings.KeyAdditionalResources];
				return SetDefaultValuesOnResource((DictObj)level2List[node.Index]);
			}
			return null;
		}

		private DictObj SetDefaultValuesOnResource(DictObj resourceData)
		{
			if(!resourceData.ContainsKey(SEBSettings.KeyAdditionalResources))
				resourceData[SEBSettings.KeyAdditionalResources] = new ListObj();
			if(!resourceData.ContainsKey(SEBSettings.KeyAdditionalResourcesActive))
				resourceData[SEBSettings.KeyAdditionalResourcesActive] = true;
			if(!resourceData.ContainsKey(SEBSettings.KeyAdditionalResourcesAutoOpen))
				resourceData[SEBSettings.KeyAdditionalResourcesAutoOpen] = false;
			if(!resourceData.ContainsKey(SEBSettings.KeyAdditionalResourcesIdentifier))
				resourceData[SEBSettings.KeyAdditionalResourcesIdentifier] = "";
			if(!resourceData.ContainsKey(SEBSettings.KeyAdditionalResourcesResourceIcons))
				resourceData[SEBSettings.KeyAdditionalResourcesResourceIcons] = new ListObj();
			if(!resourceData.ContainsKey(SEBSettings.KeyAdditionalResourcesTitle))
				resourceData[SEBSettings.KeyAdditionalResourcesTitle] = "New Resource";
			if (!resourceData.ContainsKey(SEBSettings.KeyAdditionalResourcesUrl))
				resourceData[SEBSettings.KeyAdditionalResourcesUrl] = "";
			if (!resourceData.ContainsKey(SEBSettings.KeyAdditionalResourcesURLFilterRules))
				resourceData[SEBSettings.KeyAdditionalResourcesURLFilterRules] = new ListObj();

			if (!resourceData.ContainsKey(SEBSettings.KeyAdditionalResourcesResourceData))
				resourceData[SEBSettings.KeyAdditionalResourcesResourceData] = "";
			if(!resourceData.ContainsKey(SEBSettings.KeyAdditionalResourcesResourceDataFilename))
				resourceData[SEBSettings.KeyAdditionalResourcesResourceDataFilename] = "";
			if(!resourceData.ContainsKey(SEBSettings.KeyAdditionalResourcesResourceDataLauncher))
				resourceData[SEBSettings.KeyAdditionalResourcesResourceDataLauncher] = 0;
			if (!resourceData.ContainsKey(SEBSettings.KeyAdditionalResourcesLinkUrl))
				resourceData[SEBSettings.KeyAdditionalResourcesLinkUrl] = "";
			if (!resourceData.ContainsKey(SEBSettings.KeyAdditionalResourcesRefererFilter))
				resourceData[SEBSettings.KeyAdditionalResourcesRefererFilter] = "";
			if (!resourceData.ContainsKey(SEBSettings.KeyAdditionalResourcesResetSession))
				resourceData[SEBSettings.KeyAdditionalResourcesResetSession] = false;
			if (!resourceData.ContainsKey(SEBSettings.KeyAdditionalResourcesKey))
				resourceData[SEBSettings.KeyAdditionalResourcesKey] = "";
			if (!resourceData.ContainsKey(SEBSettings.KeyAdditionalResourcesModifiers))
				resourceData[SEBSettings.KeyAdditionalResourcesModifiers] = "";
			if (!resourceData.ContainsKey(SEBSettings.KeyAdditionalResourcesConfirm))
				resourceData[SEBSettings.KeyAdditionalResourcesConfirm] = false;
			if (!resourceData.ContainsKey(SEBSettings.KeyAdditionalResourcesConfirmText))
				resourceData[SEBSettings.KeyAdditionalResourcesConfirmText] = "";
			if (!resourceData.ContainsKey(SEBSettings.KeyAdditionalResourcesShowButton))
				resourceData[SEBSettings.KeyAdditionalResourcesShowButton] = true;

			return resourceData;
		}

		private void UpdateAdditionalResourceIdentifiers()
		{
			foreach (TreeNode l0Node in treeViewAdditionalResources.Nodes)
			{
				DictObj l0resource = (DictObj)SEBSettings.additionalResourcesList[l0Node.Index];
				l0resource[SEBSettings.KeyAdditionalResourcesIdentifier] = l0Node.Index.ToString();
				foreach (TreeNode l1Node in l0Node.Nodes)
				{
					ListObj l1resources = (ListObj)l0resource[SEBSettings.KeyAdditionalResources];
					DictObj l1resource = (DictObj) l1resources[l1Node.Index];
					l1resource[SEBSettings.KeyAdditionalResourcesIdentifier] = l0Node.Index + "." + l1Node.Index;
					foreach (TreeNode l2Node in l1Node.Nodes)
					{
						ListObj l2resources = (ListObj)l1resource[SEBSettings.KeyAdditionalResources];
						DictObj l2resource = (DictObj)l2resources[l2Node.Index];
						l2resource[SEBSettings.KeyAdditionalResourcesIdentifier] = l0Node.Index + "." + l1Node.Index + "." + l2Node.Index;
					}
				}
			}
		}

		private void buttonAdditionalResourcesMoveUp_Click(object sender, EventArgs e)
		{
			var nodeToMove = treeViewAdditionalResources.SelectedNode;
			if (nodeToMove.Index == 0)
				return;

			var oldIndex = nodeToMove.Index;

			var parent = treeViewAdditionalResources.SelectedNode.Parent;
			if (parent == null)
			{
				var nodeToMoveDown = treeViewAdditionalResources.Nodes[oldIndex - 1];
				treeViewAdditionalResources.Nodes.RemoveAt(oldIndex - 1);
				treeViewAdditionalResources.Nodes.Insert(oldIndex, nodeToMoveDown);
				DictObj resourceToMoveDown = (DictObj)SEBSettings.additionalResourcesList[oldIndex - 1];
				SEBSettings.additionalResourcesList.RemoveAt(oldIndex -1);
				SEBSettings.additionalResourcesList.Insert(oldIndex, resourceToMoveDown);
			}
			else
			{
				var nodeToMoveDown = parent.Nodes[oldIndex - 1];
				parent.Nodes.RemoveAt(oldIndex - 1);
				parent.Nodes.Insert(oldIndex, nodeToMoveDown);
				DictObj parentResource = new DictObj();
				if (parent.Level == 0)
				{
					parentResource = (DictObj)SEBSettings.additionalResourcesList[parent.Index];
				}
				if (parent.Level == 1)
				{
					DictObj l0Resource = (DictObj)SEBSettings.additionalResourcesList[parent.Parent.Index];
					ListObj l0ResourcesList = (ListObj)l0Resource[SEBSettings.KeyAdditionalResources];
					parentResource = (DictObj)l0ResourcesList[parent.Index];
				}
				ListObj parentResourceList = (ListObj) parentResource[SEBSettings.KeyAdditionalResources];
				DictObj resourceToMoveDown = (DictObj)parentResourceList[oldIndex - 1];
				parentResourceList.RemoveAt(oldIndex -1);
				parentResourceList.Insert(oldIndex, resourceToMoveDown);
			}

			UpdateAdditionalResourceIdentifiers();
		}

		private void buttonAdditionalResourcesMoveDown_Click(object sender, EventArgs e)
		{
			var nodeToMove = treeViewAdditionalResources.SelectedNode;

			var oldIndex = nodeToMove.Index;

			var parent = treeViewAdditionalResources.SelectedNode.Parent;
			if (parent == null)
			{
				if (nodeToMove.Index == treeViewAdditionalResources.Nodes.Count -1)
					return;
				var nodeToMoveUp = treeViewAdditionalResources.Nodes[oldIndex + 1];
				treeViewAdditionalResources.Nodes.RemoveAt(oldIndex + 1);
				treeViewAdditionalResources.Nodes.Insert(oldIndex, nodeToMoveUp);
				DictObj resourceToMoveUp = (DictObj) SEBSettings.additionalResourcesList[oldIndex + 1];
				SEBSettings.additionalResourcesList.RemoveAt(oldIndex + 1);
				SEBSettings.additionalResourcesList.Insert(oldIndex, resourceToMoveUp);
			}
			else
			{
				if (nodeToMove.Index == parent.Nodes.Count -1 )
					return;
				var nodeToMoveUp = parent.Nodes[nodeToMove.Index + 1];
				parent.Nodes.RemoveAt(nodeToMove.Index + 1);
				parent.Nodes.Insert(oldIndex, nodeToMoveUp);
				DictObj parentResource = new DictObj();
				if (parent.Level == 0)
				{
					parentResource = (DictObj)SEBSettings.additionalResourcesList[parent.Index];
				}
				if (parent.Level == 1)
				{
					DictObj l0Resource = (DictObj)SEBSettings.additionalResourcesList[parent.Parent.Index];
					ListObj l0ResourcesList = (ListObj)l0Resource[SEBSettings.KeyAdditionalResources];
					parentResource = (DictObj)l0ResourcesList[parent.Index];
				}
				ListObj parentResourceList = (ListObj)parentResource[SEBSettings.KeyAdditionalResources];
				DictObj resourceToMoveDown = (DictObj)parentResourceList[oldIndex + 1];
				parentResourceList.RemoveAt(oldIndex + 1);
				parentResourceList.Insert(oldIndex, resourceToMoveDown);
			}

			UpdateAdditionalResourceIdentifiers();
		}

		private void buttonadditionalResourcesRemove_Click(object sender, EventArgs e)
		{
			var node = treeViewAdditionalResources.SelectedNode;

			if (node != null)
			{
				if (node.Level == 0)
				{
					SEBSettings.additionalResourcesList.RemoveAt(node.Index);
				}
				else if (node.Level == 1)
				{
					DictObj rootResource = (DictObj)SEBSettings.additionalResourcesList[node.Parent.Index];
					ListObj level1List = (ListObj)rootResource[SEBSettings.KeyAdditionalResources];
					level1List.RemoveAt(node.Index);
				}
				else if (node.Level == 2)
				{
					DictObj rootResource = (DictObj)SEBSettings.additionalResourcesList[treeViewAdditionalResources.SelectedNode.Parent.Parent.Index];
					ListObj level1List = (ListObj)rootResource[SEBSettings.KeyAdditionalResources];
					DictObj level1Resource = (DictObj)level1List[treeViewAdditionalResources.SelectedNode.Parent.Index];
					ListObj level2List = (ListObj)level1Resource[SEBSettings.KeyAdditionalResources];
					level2List.RemoveAt(node.Index);
				}
				node.Remove();

				UpdateAdditionalResourceIdentifiers();
			}

			groupBoxAdditionalResourceDetails.Visible = treeViewAdditionalResources.SelectedNode != null;
		}

		private void checkBoxAdditionalResourceActive_CheckedChanged(object sender, EventArgs e)
		{
			DictObj selectedResource = GetSelectedResource();
			selectedResource[SEBSettings.KeyAdditionalResourcesActive] = checkBoxAdditionalResourceActive.Checked;

			treeViewAdditionalResources.SelectedNode.Text = GetDisplayTitle(selectedResource);
		}

		private void checkBoxAdditionalResourceAutoOpen_CheckedChanged(object sender, EventArgs e)
		{
			DictObj selectedResource = GetSelectedResource();
			selectedResource[SEBSettings.KeyAdditionalResourcesAutoOpen] = checkBoxAdditionalResourceAutoOpen.Checked;

			treeViewAdditionalResources.SelectedNode.Text = GetDisplayTitle(selectedResource);
		}

		private void textBoxAdditionalResourcesTitle_TextChanged(object sender, EventArgs e)
		{
			DictObj selectedResource = GetSelectedResource();
			selectedResource[SEBSettings.KeyAdditionalResourcesTitle] = textBoxAdditionalResourcesTitle.Text;

			treeViewAdditionalResources.SelectedNode.Text = GetDisplayTitle(selectedResource);
		}

		private void textBoxAdditionalResourceUrl_TextChanged(object sender, EventArgs e)
		{
			DictObj selectedResource = GetSelectedResource();
			selectedResource[SEBSettings.KeyAdditionalResourcesUrl] = textBoxAdditionalResourceUrl.Text;

			treeViewAdditionalResources.SelectedNode.Text = GetDisplayTitle(selectedResource);

			buttonAdditionalResourcesChooseEmbeddedFolder.Enabled = 
				buttonAdditionalResourceChooseEmbededResource.Enabled = string.IsNullOrEmpty(textBoxAdditionalResourceUrl.Text);
		}

		private void buttonAdditionalResourceChooseEmbededResource_Click(object sender, EventArgs e)
		{
			var openFileDialog = new OpenFileDialog
			{
				CheckFileExists = true,
				CheckPathExists = true,
				Multiselect = false
			};
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				try
				{
					var fileInfo = new FileInfo(openFileDialog.FileName);
					var resourceData = _fileCompressor.CompressAndEncodeFile(openFileDialog.FileName);
					DictObj selectedResource = GetSelectedResource();

					selectedResource[SEBSettings.KeyAdditionalResourcesResourceDataFilename] = fileInfo.Name;
					selectedResource[SEBSettings.KeyAdditionalResourcesResourceData] = resourceData;
					comboBoxAdditionalResourcesChooseFileToLaunch.Visible = true; 
					comboBoxAdditionalResourcesChooseFileToLaunch.Enabled = false;
					comboBoxAdditionalResourcesChooseFileToLaunch.Text = fileInfo.Name;

					SetIconFromFile(openFileDialog.FileName);

					treeViewAdditionalResources.SelectedNode.Text = GetDisplayTitle(selectedResource);
					EmbeddedResourceChosen();
				}
				catch (OutOfMemoryException)
				{
					MessageBox.Show("The chosen resource file is too large!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void buttonAdditionalResourcesChooseEmbeddedFolder_Click(object sender, EventArgs e)
		{
			var chooseFolderDialog = new FolderBrowserDialog
			{
				ShowNewFolderButton = false,
				Description = "Choose a folder to embed, including subdirectories. Afterwards you can select the file to start when the resource is selected"
			};
			if (chooseFolderDialog.ShowDialog() == DialogResult.OK)
			{
				try
				{
					var resourceData = _fileCompressor.CompressAndEncodeDirectory(chooseFolderDialog.SelectedPath, out List<string> containingFilenames);
					DictObj selectedResource = GetSelectedResource();

					selectedResource[SEBSettings.KeyAdditionalResourcesResourceData] = resourceData;
					treeViewAdditionalResources.SelectedNode.Text = GetDisplayTitle(selectedResource);
				
					comboBoxAdditionalResourcesChooseFileToLaunch.DataSource = containingFilenames;
					comboBoxAdditionalResourcesChooseFileToLaunch.Visible = true;
					comboBoxAdditionalResourcesChooseFileToLaunch.Enabled = true;

					EmbeddedResourceChosen();
				}
				catch (OutOfMemoryException)
				{
					MessageBox.Show("The chosen resource folder is too large!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
			}
		}

		private void SetIconFromUrl(string url)
		{
			if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
				return;

			try
			{
				var uri = new Uri(textBoxAdditionalResourceUrl.Text);
				DictObj selectedResource = GetSelectedResource();

				var ic = new DictObj();
				ic[SEBSettings.KeyAdditionalResourcesResourceIconsIconData] =
					_fileCompressor.CompressAndEncodeFavicon(uri);
				ic[SEBSettings.KeyAdditionalResourcesResourceIconsFormat] = "png";

				var icons = (ListObj)selectedResource[SEBSettings.KeyAdditionalResourcesResourceIcons];
				if (icons.Count > 0)
				{
					icons[0] = ic;
				}
				else
				{
					icons.Add(ic);
				}

				var memoryStream = _fileCompressor.DeCompressAndDecode((string)ic[SEBSettings.KeyAdditionalResourcesResourceIconsIconData]);
				var image = Image.FromStream(memoryStream);
				pictureBoxAdditionalResourceIcon.Image = image;
			}
			catch (Exception ex)
			{
				Logger.AddError(string.Format("Unable to extract Icon of Url {0}", url), this, ex);
			}
		}

		private void comboBoxAdditionalResourcesChooseFileToLaunch_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (comboBoxAdditionalResourcesChooseFileToLaunch.SelectedItem != null)
			{
				DictObj selectedResource = GetSelectedResource();
				selectedResource[SEBSettings.KeyAdditionalResourcesResourceDataFilename] = comboBoxAdditionalResourcesChooseFileToLaunch.SelectedItem;
			}
		}

		private void SetIconFromFile(string filename)
		{
			if (!File.Exists(filename))
				return;
			try
			{
				var icon = Icon.ExtractAssociatedIcon(filename);
				if (icon != null)
				{
					DictObj selectedResource = GetSelectedResource();

					var ic = new DictObj();
					ic[SEBSettings.KeyAdditionalResourcesResourceIconsIconData] =
						_fileCompressor.CompressAndEncodeIcon(icon);
					ic[SEBSettings.KeyAdditionalResourcesResourceIconsFormat] = "png";

					var icons = (ListObj)selectedResource[SEBSettings.KeyAdditionalResourcesResourceIcons];
					if (icons.Count > 0)
					{
						icons[0] = ic;
					}
					else
					{
						icons.Add(ic);
					}

					var memoryStream = _fileCompressor.DeCompressAndDecode((string)ic[SEBSettings.KeyAdditionalResourcesResourceIconsIconData]);
					var image = Image.FromStream(memoryStream);
					pictureBoxAdditionalResourceIcon.Image = image;

				}
			}
			catch (Exception ex)
			{
				Logger.AddError(string.Format("Unable to extract Icon of File {0}",filename),this,ex);
			}
		}

		private void EmbeddedResourceChosen()
		{
			buttonAdditionalResourceRemoveResourceData.Visible = true;
			buttonAdditionalResourceEmbededResourceOpen.Visible = true;
			labelAdditionalResourcesResourceDataLaunchWith.Visible = true;
			labelAdditionalResourcesFilename.Visible = true;
			comboBoxAdditionalResourcesResourceDataLauncher.Visible = true;

			textBoxAdditionalResourceUrl.Text = "";
			textBoxAdditionalResourceUrl.Enabled = false;

			comboBoxAdditionalResourcesResourceDataLauncher.DataSource = GetLaunchers();
		}

		private void buttonAdditionalResourceEmbededResourceOpen_Click(object sender, EventArgs e)
		{
			DictObj selectedResource = GetSelectedResource();
			var filename = (string) selectedResource[SEBSettings.KeyAdditionalResourcesResourceDataFilename];
			var path =
				_fileCompressor.DecompressDecodeAndSaveFile(
					(string)selectedResource[SEBSettings.KeyAdditionalResourcesResourceData], filename, selectedResource[SEBSettings.KeyAdditionalResourcesIdentifier].ToString());
			Process.Start(path + filename);
		}

		private void buttonAdditionalResourceRemoveResourceData_Click(object sender, EventArgs e)
		{
			DictObj selectedResource = GetSelectedResource();

			selectedResource[SEBSettings.KeyAdditionalResourcesResourceData] = "";
			selectedResource[SEBSettings.KeyAdditionalResourcesResourceIconsIconData] = "";
			selectedResource[SEBSettings.KeyAdditionalResourcesResourceDataFilename] = "";
			selectedResource[SEBSettings.KeyAdditionalResourcesResourceDataLauncher] = 0;
			
			treeViewAdditionalResources.SelectedNode.Text = GetDisplayTitle(selectedResource);

			buttonAdditionalResourceRemoveResourceData.Visible = false;
			buttonAdditionalResourceEmbededResourceOpen.Visible = false;
			labelAdditionalResourcesResourceDataLaunchWith.Visible = false;
			labelAdditionalResourcesFilename.Visible = false;
			comboBoxAdditionalResourcesResourceDataLauncher.Visible = false;
			comboBoxAdditionalResourcesChooseFileToLaunch.Visible = false;

			pictureBoxAdditionalResourceIcon.Image = null;

			textBoxAdditionalResourceUrl.Enabled = true;
		}

		private void comboBoxAdditionalResourcesResourceDataLauncher_SelectedIndexChanged(object sender, EventArgs e)
		{
			DictObj selectedResource = GetSelectedResource();
			if ((int) selectedResource[SEBSettings.KeyAdditionalResourcesResourceDataLauncher] !=
				comboBoxAdditionalResourcesResourceDataLauncher.SelectedIndex)
			{
				selectedResource[SEBSettings.KeyAdditionalResourcesResourceDataLauncher] =
					comboBoxAdditionalResourcesResourceDataLauncher.SelectedIndex;
			}
		}

		private void buttonAdditionalResourcesChooseIcon_Click(object sender, EventArgs e)
		{
			var openFileDialog = new OpenFileDialog
			{
				CheckFileExists = true,
				CheckPathExists = true,
				Multiselect = false,
				Filter = "PNG Images|*.png"
			};
			if (openFileDialog.ShowDialog() == DialogResult.OK)
			{
				DictObj selectedResource = GetSelectedResource();

				var icon = new DictObj();
				icon[SEBSettings.KeyAdditionalResourcesResourceIconsIconData] =
					_fileCompressor.CompressAndEncodeFile(openFileDialog.FileName);
				icon[SEBSettings.KeyAdditionalResourcesResourceIconsFormat] = "png";

				var icons = (ListObj)selectedResource[SEBSettings.KeyAdditionalResourcesResourceIcons];
				if (icons.Count > 0)
				{
					icons[0] = icon;
				}
				else
				{
					icons.Add(icon);   
				}

				var memoryStream = _fileCompressor.DeCompressAndDecode((string)icon[SEBSettings.KeyAdditionalResourcesResourceIconsIconData]);
				var image = Image.FromStream(memoryStream);
				pictureBoxAdditionalResourceIcon.Image = image;
			}
		}

		private void textBoxAdditionalResourceUrl_Leave(object sender, EventArgs e)
		{
			SetIconFromUrl(textBoxAdditionalResourceUrl.Text);
			CreateURLFilterRule(textBoxAdditionalResourceUrl.Text);

			// Make sure URL filter is enabled and show message box if not
			if ((Boolean)SEBSettings.settingsCurrent[SEBSettings.KeyURLFilterEnable] == false)
			{
				SEBSettings.settingsCurrent[SEBSettings.KeyURLFilterEnable] = true;
				MessageBox.Show("URL Filter Enabled", "When adding an external additional resource, an according URL filter must be defined " +
					"and URL filtering enabled. You can edit the filter(s) for the resource in its URL Filter list. " +
					"You may also have to create filter rules for your exam in Network/Filter settings (SEB internally only creates a rule exactly matching the Start URL). " +
					"For full control of displayed content, 'Filter also embedded content' (Network/Filter tab) should be activated as well.", MessageBoxButtons.OK, MessageBoxIcon.Warning);
			}
		}

		private void CreateURLFilterRule(string resourceURLString)
		{
			urlFilter.UpdateFilterRules();

			// Check if resource URL gets allowed by current filter rules and if not, add a rule for the resource URL
			if (Uri.TryCreate(resourceURLString, UriKind.Absolute, out Uri resourceURL))
			{
				if (urlFilter.TestURLAllowed(resourceURL) != URLFilterRuleActions.allow)
				{
					// If resource URL is not allowed: Create one using the full resource URL
					DictObj selectedResource = GetSelectedResource();
					ListObj resourceURLFilterRules;
					if (selectedResource.TryGetValue(SEBSettings.KeyURLFilterRules, out object keyURLFilterRulesValue))
					{
						resourceURLFilterRules = (ListObj)keyURLFilterRulesValue;
					} else
					{
						resourceURLFilterRules = new ListObj();
					}
					DictObj newURLFilterRule = new DictObj();
					newURLFilterRule.Add(SEBSettings.KeyURLFilterRuleAction, (int)URLFilterRuleActions.allow);
					newURLFilterRule.Add(SEBSettings.KeyURLFilterRuleActive, true);
					newURLFilterRule.Add(SEBSettings.KeyURLFilterRuleExpression, resourceURLString);
					newURLFilterRule.Add(SEBSettings.KeyURLFilterRuleRegex, false);
					// Add this resource URL allow rule to the URL filters of this additional resource
					resourceURLFilterRules.Add(newURLFilterRule);
					selectedResource[SEBSettings.KeyURLFilterRules] = resourceURLFilterRules;
					// Update UI
					InitializeUrlFilters(selectedResource);
				}
			}
		}

		private void checkBoxShowButton_CheckedChanged(object sender, EventArgs e)
		{
			DictObj selectedResource = GetSelectedResource();
			selectedResource[SEBSettings.KeyAdditionalResourcesShowButton] = checkBoxShowButton.Checked;

			treeViewAdditionalResources.SelectedNode.Text = GetDisplayTitle(selectedResource);
		}

		private void textBoxLinkURL_TextChanged(object sender, EventArgs e)
		{
			DictObj selectedResource = GetSelectedResource();
			selectedResource[SEBSettings.KeyAdditionalResourcesLinkUrl] = textBoxLinkURL.Text;
		}

		private void checkBoxResetSession_CheckedChanged(object sender, EventArgs e)
		{
			DictObj selectedResource = GetSelectedResource();
			selectedResource[SEBSettings.KeyAdditionalResourcesResetSession] = checkBoxResetSession.Checked;
		}

		private void textBoxRefererFilter_TextChanged(object sender, EventArgs e)
		{
			DictObj selectedResource = GetSelectedResource();
			selectedResource[SEBSettings.KeyAdditionalResourcesRefererFilter] = textBoxRefererFilter.Text;
		}

		private void textBoxKey_TextChanged(object sender, EventArgs e)
		{
			DictObj selectedResource = GetSelectedResource();
			selectedResource[SEBSettings.KeyAdditionalResourcesKey] = textBoxKey.Text;
		}

		private void comboBoxModifiers_SelectedIndexChanged(object sender, EventArgs e)
		{
			DictObj selectedResource = GetSelectedResource();
			selectedResource[SEBSettings.KeyAdditionalResourcesModifiers] = comboBoxModifiers.SelectedItem;
		}

		private void checkBoxConfirm_CheckedChanged(object sender, EventArgs e)
		{
			DictObj selectedResource = GetSelectedResource();
			selectedResource[SEBSettings.KeyAdditionalResourcesConfirm] = checkBoxConfirm.Checked;
		}

		private void textBoxConfirmBoxText_TextChanged(object sender, EventArgs e)
		{
			DictObj selectedResource = GetSelectedResource();
			selectedResource[SEBSettings.KeyAdditionalResourcesConfirmText] = textBoxConfirmBoxText.Text;
		}
	}
}
