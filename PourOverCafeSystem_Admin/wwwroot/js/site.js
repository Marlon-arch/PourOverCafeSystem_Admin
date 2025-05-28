console.log("site.js is running");

const connection = new signalR.HubConnectionBuilder()
    .withUrl("/reservationHub")
    .build();

connection.on("ReceiveUpdate", function (message) {
    console.log("SignalR message received:", message);

    if (message === "RefreshDashboard") {
        updateDashboardSections();
    }
});

connection.start().catch(function (err) {
    console.error("SignalR connection error:", err.toString());
});

connection.on("WaitlistUpdated", function () {
    console.log("Waitlist update received — refreshing active/inactive cards...");
    updateDashboardSections();
});

function updateDashboardSections() {
    fetch("/Dashboard/Index")
        .then(response => response.text())
        .then(html => {
            const parser = new DOMParser();
            const doc = parser.parseFromString(html, "text/html");

            const newApprovals = doc.querySelector("#approvals");
            const newActive = doc.querySelector("#waitlist-active");
            const newInactive = doc.querySelector("#waitlist-inactive");

            if (newApprovals && newActive && newInactive) {
                document.querySelector("#approvals").innerHTML = newApprovals.innerHTML;
                document.querySelector("#waitlist-active").innerHTML = newActive.innerHTML;
                document.querySelector("#waitlist-inactive").innerHTML = newInactive.innerHTML;

                console.log("Dashboard sections updated via SignalR.");

                // Re-apply removed card hiding for inactive cards
                const cards = document.querySelectorAll(".inactive-card");
                cards.forEach(card => {
                    const id = card.getAttribute("data-id");
                    if (localStorage.getItem("removedCard_" + id) === "true") {
                        card.style.display = "none";
                    }
                });
            }
        });
}

