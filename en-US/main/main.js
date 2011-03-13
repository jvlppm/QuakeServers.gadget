function showFlyout(page, object) {
	System.Gadget.Flyout.file = page;
	System.Gadget.Flyout.show = true;

	var flyoutWin = System.Gadget.Flyout.document.parentWindow;
	flyoutWin.Wrapper = Wrapper;
	flyoutWin.object = object;
}

function UpdateView() {
	var servers = eval(Wrapper.Servers);
	for (var i = 0; i < servers.length; i++) {
		var currentServer = servers[i];

		var serverDiv = $("#main > div[server_ip='" + currentServer.Ip + "']");

		if (!serverDiv.length) {
			serverDiv = $(
			"<div class='linha_tabela' server_ip='" + currentServer.Ip + "'>"
			+ "<div class='server_name'></div>"
			+ "<div class='server_info'></div>"
			+"</div>");

			if (i % 2)
				serverDiv.addClass("zebra_off");
			else
				serverDiv.addClass("zebra_on");

			$("#main").append(serverDiv);
			serverDiv.get(0).onclick = function () {
				showFlyout("main/serverInfo/serverInfo.html", this.object);
			};
		}
		serverDiv.get(0).object = currentServer;
		serverDiv.find(" > .server_name").html(currentServer.Name);
		serverDiv.find(" > .server_info").html(currentServer.NumberOfPlayers + " players");

		if (currentServer.NumberOfPlayers > 0)
			serverDiv.addClass("has_players");
		else serverDiv.removeClass("has_players");
	}
}

$(document).ready(function () {
	System.Gadget.settingsUI = "main/settings/settings.html";
	System.Gadget.document.Wrapper = Wrapper;
	Wrapper.RootPath = System.Gadget.path;

	UpdateView();
	Wrapper.UpdateServers();
	setInterval(function () { Wrapper.UpdateServers(); }, 10000);

	setInterval(UpdateView, 1000);
});
