// --- Browser API Abstraction ---
const isFirefox = typeof browser !== "undefined";
const api = isFirefox ? browser : chrome;

const actionApi = api.action || api.browserAction || {};

function getStorage(keys) {
  if (isFirefox) return api.storage.local.get(keys);
  return new Promise((resolve) => {
    api.storage.local.get(keys, (result) => resolve(result));
  });
}

function setStorage(items) {
  if (isFirefox) return api.storage.local.set(items);
  return new Promise((resolve) => {
    api.storage.local.set(items, () => resolve());
  });
}

// --- State Management ---
async function setInitialState() {
  const data = await getStorage("isEnabled");
  if (data.isEnabled === undefined) {
    await setStorage({ isEnabled: true });
    updateAction(true);
  } else {
    updateAction(data.isEnabled);
  }
}

// --- Action / Badge Management (service worker safe) ---
function updateAction(isEnabled) {
  const title = isEnabled ? "AST (Active)" : "AST (Inactive)";
  const text = isEnabled ? "ON" : "OFF";
  const color = isEnabled ? [40,167,69,255] : [220,53,69,255];

  try {
    if (actionApi.setBadgeText) actionApi.setBadgeText({ text });
    if (actionApi.setBadgeBackgroundColor) actionApi.setBadgeBackgroundColor({ color });
    if (actionApi.setTitle) actionApi.setTitle({ title });
  } catch (err) {
    console.error('AST: Could not update action/badge', err);
  }
}

// --- Event Listeners ---
api.runtime.onInstalled.addListener(setInitialState);
api.runtime.onStartup.addListener(setInitialState);

const addActionClickListener = () => {
  if (api.action && api.action.onClicked) {
    api.action.onClicked.addListener(toggleStateFromAction);
  } else if (api.browserAction && api.browserAction.onClicked) {
    api.browserAction.onClicked.addListener(toggleStateFromAction);
  }
};

async function toggleStateFromAction() {
  const data = await getStorage("isEnabled");
  const newState = !data.isEnabled;
  await setStorage({ isEnabled: newState });
  updateAction(newState);
  notifyContentScripts(newState);
}

addActionClickListener();

// --- Communication with Content Scripts ---
function notifyContentScripts(isEnabled) {
  // Query all tabs and send a message to each; ignore tabs without IDs
  api.tabs.query({}, (tabs) => {
    tabs.forEach((tab) => {
      if (!tab.id) return;
      try {
        if (isFirefox) {
          api.tabs.sendMessage(tab.id, { isEnabled }).catch(() => {});
        } else {
          api.tabs.sendMessage(tab.id, { isEnabled }, () => {});
        }
      } catch (e) {
        // ignore tabs that can't be messaged
      }
    });
  });
}

api.runtime.onMessage.addListener((message, sender, sendResponse) => {
  if (message.request === "getState") {
    getStorage("isEnabled").then(data => sendResponse({ isEnabled: data.isEnabled }));
    return true; // indicate async response
  }
});
