/// <reference no-default-lib="true"/>
/// <reference lib="es2015" />
/// <reference lib="webworker" />
const port = chrome.runtime.connectNative("native_test_host");
port.onMessage.addListener(function (msg) {
    port.postMessage({ text: "hello word" });
    console.log("Received %o", msg);
});
chrome.runtime.onInstalled.addListener(async () => {
    console.log(self);
});
chrome.runtime.onConnect.addListener(p => {
    console.info("new connection " + p.name);
    p.onMessage.addListener(m => {
        console.info("message " + m.value);
        p.postMessage("pong");
    });
});
// chrome.tabs.executeScirpt({file: "./alert.js", runAt: "document_start"})
//# sourceMappingURL=background.js.map