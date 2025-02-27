/*
 * Copyright (c) 2025 ETH Zürich, IT Services
 * 
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */

function disableMouseWheelZoom(e) {
	if (e.ctrlKey) {
		e.preventDefault();
		e.stopPropagation();
	}
}

document.addEventListener('wheel', disableMouseWheelZoom, { passive: false });