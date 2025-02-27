/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 * 
 * Original code taken and slightly adapted from https://github.com/eqsoft/seb2/blob/master/browser/app/modules/SebBrowser.jsm#L1215.
 */

SafeExamBrowser.clipboard = {
	id: Math.round((Date.now() + Math.random()) * 1000),
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

function copySelectedData(e) {
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

function cutSelectedData(e) {
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

function pasteSelectedData(e) {
	if (e.target.contentEditable && e.target.setRangeText) {
		e.target.setRangeText("", e.target.selectionStart, e.target.selectionEnd, 'select');
		e.target.setRangeText(SafeExamBrowser.clipboard.text, e.target.selectionStart, e.target.selectionStart + SafeExamBrowser.clipboard.text.length, 'end');
	} else {
		var w = e.target.ownerDocument.defaultView;
		var designMode = e.target.ownerDocument.designMode;
		var contentEditables = e.target.ownerDocument.querySelectorAll('*[contenteditable]');
		var selection = w.getSelection();

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
			var range = w.getSelection().getRangeAt(0);

			if (SafeExamBrowser.clipboard.ranges.length > 0) {
				SafeExamBrowser.clipboard.ranges.map(r => {
					range = w.getSelection().getRangeAt(0);
					range.collapse();
					const newNode = r.cloneNode(true);
					range.insertNode(newNode);
					range.collapse();
				});
			} else {
				range.collapse();
				range.insertNode(w.document.createTextNode(SafeExamBrowser.clipboard.text));
				range.collapse();
			}
		} else {
			if (contentEditables.length) {
				contentEditables.forEach(node => {
					var range = w.getSelection().getRangeAt(0);

					if (node.contains(range.commonAncestorContainer)) {
						if (SafeExamBrowser.clipboard.ranges.length > 0) {
							SafeExamBrowser.clipboard.ranges.map(r => {
								range = w.getSelection().getRangeAt(0);
								range.collapse();
								const newNode = r.cloneNode(true);
								range.insertNode(newNode);
								range.collapse();
							});
						} else {
							range = w.getSelection().getRangeAt(0);
							range.collapse();
							range.insertNode(w.document.createTextNode(SafeExamBrowser.clipboard.text));
							range.collapse();
						}
					}
				});
			}
		}
	}
}

function onCopy(e) {
	SafeExamBrowser.clipboard.clear();

	try {
		copySelectedData(e);

		CefSharp.PostMessage({ Type: "Clipboard", Id: SafeExamBrowser.clipboard.id, Content: SafeExamBrowser.clipboard.getContentEncoded() });
	} finally {
		e.preventDefault();
		e.returnValue = false;
	}

	return false;
}

function onCut(e) {
	SafeExamBrowser.clipboard.clear();

	try {
		copySelectedData(e);
		cutSelectedData(e);

		CefSharp.PostMessage({ Type: "Clipboard", Id: SafeExamBrowser.clipboard.id, Content: SafeExamBrowser.clipboard.getContentEncoded() });
	} finally {
		e.preventDefault();
		e.returnValue = false;
	}

	return false;
}

function onPaste(e) {
	try {
		pasteSelectedData(e);
	} finally {
		e.preventDefault();
		e.returnValue = false;
	}

	return false;
}

window.document.addEventListener("copy", onCopy, true);
window.document.addEventListener("cut", onCut, true);
window.document.addEventListener("paste", onPaste, true);
