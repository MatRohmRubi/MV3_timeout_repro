const btn = document.createElement("button");
document.body.prepend(btn);
btn.innerText = "Send message";
const el = document.createElement("ul");
document.body.prepend(el);
function log(message) {
    const li = document.createElement("li");
    li.innerText = message;
    el.appendChild(li);
}
log("initialized");
const port2 = chrome.runtime.connect({
    name: "test"
});
log("connected");
btn.addEventListener("click", () => {
    log("ping");
    port2.postMessage("ping");
});
port2.onDisconnect.addListener((p) => {
    log("disconnected");
});
port2.onMessage.addListener((p) => {
    log("message: " + p);
});
//# sourceMappingURL=content.js.map