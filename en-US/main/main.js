function showFlyout(page, object) {
	System.Gadget.Flyout.file = page;
	System.Gadget.Flyout.show = true;

	var flyoutWin = System.Gadget.Flyout.document.parentWindow;
	flyoutWin.Wrapper = Wrapper;
	flyoutWin.object = object;
}

function updateSizes() {
	$("div, table").each(function () {
		$(this).width($(this).parent().width() - $(this).css("left").replace("px", "") - $(this).css("right").replace("px", ""));

		if ($(this).is("table")) {
			Wrapper = Wrapper;
		}

		if (!$(this).css("top") || $(this).css("top") == "auto")
			$(this).css("top", $(this).parent().height() - $(this).height());

		$(this).height($(this).parent().height() - $(this).css("top").replace("px", "") - $(this).css("bottom").replace("px", ""));
	});
}

function UpdateView() {
	try {
		$("#main > div").addClass("server_down");

		var servers = eval(Wrapper.Servers);
		for (var i = 0; i < servers.length; i++) {
			var currentServer = servers[i];

			var serverDiv = $("#main > div[server_ip='" + currentServer.Ip + "']");

			if (!serverDiv.length) {
				serverDiv =
			$("<div class='linha_tabela' server_ip='" + currentServer.Ip + "'>"
			+ "<div class='server_name'></div>"
			+ "<div class='server_info'></div>"
			+ "</div>"
			
			
			+"<div class='linha_tabela' server_ip='" + currentServer.Ip + "'>"
			+ "<div class='server_name'></div>"
			+ "<div class='server_info'></div>"
			+ "</div>");

				if (i % 2)
					serverDiv.addClass("zebra_off");
				else
					serverDiv.addClass("zebra_on");

				$("#main").append(serverDiv);
				serverDiv.get(0).onclick = function () {
					showFlyout("main/serverInfo/serverInfo.html", this.object);
				};
			} else {
				serverDiv.removeClass("server_down");
			}
			serverDiv.get(0).object = currentServer;
			serverDiv.find(" > .server_name").html(currentServer.Name);
			serverDiv.find(" > .server_info").html(currentServer.NumberOfPlayers + " players");
			if (currentServer.LastError)
				serverDiv.addClass("server_error");
			else serverDiv.removeClass("server_error");

			if (currentServer.NumberOfPlayers > 0)
				serverDiv.addClass("has_players");
			else serverDiv.removeClass("has_players");

			if (!Wrapper.IsPlaying && Wrapper.GamePath && Wrapper.AutoLaunch) {
				if (currentServer.NumberOfPlayers >= Wrapper.AutoLaunchMinPlayers) {
					if (Wrapper.MinutesSinceLastPlay >= Wrapper.AutoLaunchMinTime) {
						Wrapper.LaunchGame(currentServer.Ip);
					}
				}
			}
		}

		$("#main > div.server_down").remove();
		updateSizes();
	}
	catch (Exception) {
		ShowError(Exception);
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
