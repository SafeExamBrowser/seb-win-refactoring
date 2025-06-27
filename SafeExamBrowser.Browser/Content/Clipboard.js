/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 * Original code taken and adapted from https://github.com/eqsoft/seb2/blob/master/browser/app/modules/SebBrowser.jsm#L1215.
 */

if (typeof SafeExamBrowser.clipboard === 'undefined') {
	SafeExamBrowser.clipboard = {
		id: crypto.randomUUID(),
		ranges: [],
		text: "",

		clear: function () {
			this.ranges = [];
			this.text = "";
		},

		getContentEncoded: function () {
			var bytes = new TextEncoder().encode(this.text);
			var base64 = btoa(String.fromCodePoint(...bytes));

			return base64;
		},

		update: function (id, base64) {
			if (this.id != id) {
				var bytes = Uint8Array.from(atob(base64), (m) => m.codePointAt(0));
				var content = new TextDecoder().decode(bytes);

				this.ranges = [];
				this.text = content;
			}
		}
	}
}

if (typeof copySelection === 'undefined') {
	function copySelection(e) {
		if (e.target.contentEditable && e.target.setRangeText) {
			SafeExamBrowser.clipboard.text = e.target.value.substring(e.target.selectionStart, e.target.selectionEnd);
			SafeExamBrowser.clipboard.ranges = [];
		} else {
			var selection = e.target.ownerDocument.defaultView.getSelection();
			var text = "";

			for (var i = 0; i < selection.rangeCount; i++) {
				SafeExamBrowser.clipboard.ranges[i] = selection.getRangeAt(i).cloneContents();
				text += SafeExamBrowser.clipboard.ranges[i].textContent;
			}

			SafeExamBrowser.clipboard.text = text;
		}
	}
}

if (typeof cutSelection === 'undefined') {
	function cutSelection(e) {
		if (e.target.contentEditable && e.target.setRangeText) {
			e.target.setRangeText("", e.target.selectionStart, e.target.selectionEnd, 'select');
		} else {
			var designMode = e.target.ownerDocument.designMode;
			var contentEditables = e.target.ownerDocument.querySelectorAll('*[contenteditable]');
			var selection = e.target.ownerDocument.defaultView.getSelection();

			for (var i = 0; i < selection.rangeCount; i++) {
				var range = selection.getRangeAt(i);

				if (designMode === 'on') {
					range.deleteContents();
				} else {
					if (contentEditables.length) {
						contentEditables.forEach(node => {
							if (node.contains(range.commonAncestorContainer)) {
								range.deleteContents();
							}
						});
					}
				}
			}
		}
	}
}

if (typeof pasteContent === 'undefined') {
	function pasteContent(e) {
		if (e.target.contentEditable && e.target.setRangeText) {
			e.target.setRangeText("", e.target.selectionStart, e.target.selectionEnd, 'select');
			e.target.setRangeText(SafeExamBrowser.clipboard.text, e.target.selectionStart, e.target.selectionStart + SafeExamBrowser.clipboard.text.length, 'end');
		} else {
			var targetWindow = e.target.ownerDocument.defaultView;
			var designMode = e.target.ownerDocument.designMode;
			var contentEditables = e.target.ownerDocument.querySelectorAll('*[contenteditable]');
			var selection = targetWindow.getSelection();

			for (var i = 0; i < selection.rangeCount; i++) {
				var r = selection.getRangeAt(i);

				if (designMode === 'on') {
					r.deleteContents();
				} else {
					if (contentEditables.length) {
						contentEditables.forEach(node => {
							if (node.contains(r.commonAncestorContainer)) {
								r.deleteContents();
							}
						});
					}
				}
			}

			if (designMode === 'on') {
				var range = targetWindow.getSelection().getRangeAt(0);

				if (SafeExamBrowser.clipboard.ranges.length > 0) {
					SafeExamBrowser.clipboard.ranges.map(r => {
						range = targetWindow.getSelection().getRangeAt(0);
						range.collapse();
						const newNode = r.cloneNode(true);
						range.insertNode(newNode);
						range.collapse();
					});
				} else {
					range.collapse();
					range.insertNode(targetWindow.document.createTextNode(SafeExamBrowser.clipboard.text));
					range.collapse();
				}
			} else {

				if (contentEditables.length) {
					contentEditables.forEach(node => {
						var range = targetWindow.getSelection().getRangeAt(0);

						if (node.contains(range.commonAncestorContainer)) {
							if (SafeExamBrowser.clipboard.ranges.length > 0) {

								SafeExamBrowser.clipboard.ranges.map(r => {
									range = targetWindow.getSelection().getRangeAt(0);
									range.collapse();
									const newNode = r.cloneNode(true);
									range.insertNode(newNode);
									range.collapse();
								});
							} else {
								range = targetWindow.getSelection().getRangeAt(0);
								range.collapse();
								range.insertNode(targetWindow.document.createTextNode(SafeExamBrowser.clipboard.text));
								range.collapse();
							}
						}
					});
				}
			}
		}
	}
}

if (typeof onCopy === 'undefined') {
	function onCopy(e) {
		try {
			SafeExamBrowser.clipboard.clear();

			copySelection(e);

			CefSharp.PostMessage({ Type: "Clipboard", Id: SafeExamBrowser.clipboard.id, Content: SafeExamBrowser.clipboard.getContentEncoded() });
		} finally {
			e.preventDefault();
		}

		return false;
	}

	window.document.addEventListener("copy", onCopy, true);
}

if (typeof onCut === 'undefined') {
	function onCut(e) {
		try {
			SafeExamBrowser.clipboard.clear();

			copySelection(e);
			cutSelection(e);

			CefSharp.PostMessage({ Type: "Clipboard", Id: SafeExamBrowser.clipboard.id, Content: SafeExamBrowser.clipboard.getContentEncoded() });
		} finally {
			e.preventDefault();
		}

		return false;
	}

	window.document.addEventListener("cut", onCut, true);
}


if (typeof onPaste === 'undefined') {
	function onPaste(e) {
		try {
			pasteContent(e);
		} finally {
			e.preventDefault();
		}

		return false;
	}

	window.document.addEventListener("paste", onPaste, true);
}
