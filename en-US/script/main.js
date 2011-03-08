var checkForUpdatesMinutes = 5;

function showFlyout(page, object) {
	System.Gadget.Flyout.file = page;
	System.Gadget.Flyout.show = true;

	var flyoutWin = System.Gadget.Flyout.document.parentWindow;
	flyoutWin.object = object;
}

function UpdateView() {
	$("#main").html("");
	var servers = eval(Wrapper.Servers);
	for (var i = 0; i < servers.length; i++) {
		var currentServer = servers[i];
		var serverDiv = $("<div>" + servers[i].Name + " -> " + servers[i].NumberOfPlayers + " players</div>");
		$("#main").append(serverDiv);
		serverDiv.get(0).onclick = function () {
			showFlyout("serverInfo.html", currentServer);
		};
	}
}

$(document).ready(function () {
	UpdateView();
	Wrapper.UpdateServers();

	setInterval(UpdateView, 1000);
	setInterval(function () { Wrapper.UpdateServers(); }, 10000);

	setInterval(CheckForUpdates, checkForUpdatesMinutes * 60 * 1000);

	$("#main > div").each(function () {
		var serverDiv = $(this);
		serverDiv.get(0).onclick = function () {

		};
	});
});
