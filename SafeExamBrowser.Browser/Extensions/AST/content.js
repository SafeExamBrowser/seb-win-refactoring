// AST: Bulletproof Main-World Shield
(function() {
    'use strict';

    console.log("AST: Establishing Main-World Shield...");

    const blockedEvents = [
        'blur', 'focus', 'focusin', 'focusout', 
        'visibilitychange', 'webkitvisibilitychange', 'mozvisibilitychange', 
        'pagehide', 'pageshow', 'mouseleave', 'mouseenter'
    ];

    // 1. Prototype Hijack: Block site-level event listeners
    const originalAddEventListener = EventTarget.prototype.addEventListener;
    EventTarget.prototype.addEventListener = function(type, listener, options) {
        if (blockedEvents.includes(type)) {
            // console.log("AST: Blocked site listener for: " + type);
            return;
        }
        return originalAddEventListener.apply(this, arguments);
    };

    // 2. Prototype Hijack: Block manual event triggering
    const originalDispatchEvent = EventTarget.prototype.dispatchEvent;
    EventTarget.prototype.dispatchEvent = function(event) {
        if (blockedEvents.includes(event?.type)) {
            return true;
        }
        return originalDispatchEvent.apply(this, arguments);
    };

    // 3. Spoof State Properties at Prototype Level
    Object.defineProperty(Document.prototype, 'visibilityState', { get: () => 'visible', configurable: false });
    Object.defineProperty(Document.prototype, 'hidden', { get: () => false, configurable: false });
    Object.defineProperty(Document.prototype, 'hasFocus', { value: function() { return true; }, configurable: false });
    Object.defineProperty(Document.prototype, 'activeElement', { get: () => document.body, configurable: false });

    // 4. Method Neutralization
    const noop = () => {};
    Window.prototype.blur = noop;
    Window.prototype.focus = noop;
    HTMLElement.prototype.blur = noop;
    HTMLElement.prototype.focus = noop;

    // 5. Kill Legacy 'on' handlers
    const killOnEvents = (obj) => {
        blockedEvents.forEach(type => {
            try {
                Object.defineProperty(obj, 'on' + type, {
                    get: () => null,
                    set: () => {},
                    configurable: false
                });
            } catch (e) {}
        });
    };
    killOnEvents(Window.prototype);
    killOnEvents(Document.prototype);
    killOnEvents(HTMLElement.prototype);

    // 6. Capture-Phase System Event Killer
    // Use the original method to ensure our killer runs even if others try to hijack
    blockedEvents.forEach(type => {
        originalAddEventListener.call(window, type, e => {
            e.stopImmediatePropagation();
            e.stopPropagation();
        }, true);
        originalAddEventListener.call(document, type, e => {
            e.stopImmediatePropagation();
            e.stopPropagation();
        }, true);
    });

    // 7. Feature Restoration (Clipboard)
    const clipboardEvents = ['copy', 'paste', 'cut', 'contextmenu'];
    const originalPreventDefault = Event.prototype.preventDefault;
    Event.prototype.preventDefault = function() {
        if (clipboardEvents.includes(this.type)) return;
        return originalPreventDefault.apply(this, arguments);
    };

    clipboardEvents.forEach(type => {
        originalAddEventListener.call(window, type, e => e.stopImmediatePropagation(), true);
    });

    console.log("AST: Main-World Shield Fully Established.");
})();
