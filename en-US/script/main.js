function showFlyout(page, object) {
	System.Gadget.Flyout.file = page;
	System.Gadget.Flyout.show = true;

	var flyoutWin = System.Gadget.Flyout.document.parentWindow;
	flyoutWin.Wrapper = Wrapper;
	flyoutWin.object = object;
}

function UpdateView() {
	$("#main").html("");
	var servers = eval(Wrapper.Servers);
	for (var i = 0; i < servers.length; i++) {
		var currentServer = servers[i];
		var serverDiv = $("<div class='linha_tabela'>" + servers[i].Name + " -> " + servers[i].NumberOfPlayers + " players</div>");

		if(i % 2)
			serverDiv.addClass("zebra_off");
		else
			serverDiv.addClass("zebra_on");

		if (currentServer.NumberOfPlayers > 0)
			serverDiv.addClass("has_players");

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

	$("#main > div").each(function () {
		var serverDiv = $(this);
		serverDiv.get(0).onclick = function () {

		};
	});
});
