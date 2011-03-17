function showFlyout(page, object) {
	System.Gadget.Flyout.file = page;
	System.Gadget.Flyout.show = true;

	var flyoutWin = System.Gadget.Flyout.document.parentWindow;
	flyoutWin.Wrapper = Wrapper;
	flyoutWin.object = object;
}

function updateSize() {
	$("div, table").each(function () {
		$(this).width($(this).parent().width() - $(this).css("left").replace("px", "") - $(this).css("right").replace("px", ""));

		if ($(this).is("table")) {
			Wrapper = Wrapper;
		}

		var top = $(this).css("top");

		if (!top || top == "auto")
			top = ($(this).parent().height() - $(this).height()) + "px";

		$(this).height($(this).parent().height() - top.replace("px", "") - $(this).css("bottom").replace("px", ""));
	});

	updatePageNav();
}

function updatePageNav() {
	var prev;
	var next;
	if ($("#main").attr("scrollTop") <= 0) {
		$("#page_prev").css("cursor", "default");
		$("#page_prev").attr("src", "images/page_prev_gray.png");
		$("#page_prev").get(0).onclick = null;
	}
	else {
		prev = true;
		$("#page_prev").css("cursor", "hand");
		$("#page_prev").attr("src", "images/page_prev.png");
		$("#page_prev").get(0).onclick = goToPreviousPage;
	}

	if ($("#main").attr("scrollTop") >= ($("#main").attr("scrollHeight") - $("#main").height())) {
		$("#page_next").css("cursor", "default");
		$("#page_next").attr("src", "images/page_next_gray.png");
		$("#page_next").get(0).onclick = null;
	}
	else {
		next = true;
		$("#page_next").css("cursor", "hand");
		$("#page_next").attr("src", "images/page_next.png");
		$("#page_next").get(0).onclick = goToNextPage;
	}
	if (!prev && !next) {
		$("#nav").hide();
	}
	else {
		$("#nav").show();
	}
}

function goToPreviousPage() {
	var firstTop = $("#main > *:eq(0)").offset().top;

	var nextTop = $("#main").attr("scrollTop") - $("#main").height();
	var lastTop = $("#main").attr("scrollHeight") - $("#main").height();

	var nextPageEls = $("#main > *").filter(function (i) {
		var e = $("#main > *:eq(" + i + ")");
		var elTop = e.offset().top - firstTop;
		return (nextTop - e.height()) < elTop;
	});

	if (!nextPageEls.length) {
		nextTop = 0;
	}
	else {
		nextTop = nextPageEls.first().offset().top - firstTop;
	}

	$("#main").animate({ scrollTop: nextTop },
	{
		duration: 500,
		complete: updatePageNav
	});
}

function goToNextPage() {
	var firstTop = $("#main > *:eq(0)").offset().top;

	var nextTop = $("#main").attr("scrollTop") + $("#main").height();
	var lastTop = $("#main").attr("scrollHeight") - $("#main").height();

	var nextPageEls = $("#main > *").filter(function (i) {
		var e = $("#main > *:eq(" + i + ")");
		return (e.offset().top - firstTop) + e.height() + parseInt(e.css("margin-bottom").replace("px", "")) >= nextTop;
	});

	if (nextTop > lastTop)
		nextTop = lastTop;
	else {
		if (!nextPageEls.length) {
			nextTop = lastTop;
		}
		else {
			nextTop = nextPageEls.first().offset().top - firstTop;
		} 
	}
	
	$("#main").animate({ scrollTop: nextTop },
	{
		duration: 500,
		complete: updatePageNav
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
		updateSize();
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
	setInterval(function () {
		try {
			Wrapper.UpdateServers();
		} catch (Exception) {
			ShowError(Exception);
		}
	}, 10000);

	setInterval(UpdateView, 1000);
});
