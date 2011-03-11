var updatingView = false;

function updateView() {
	if (updatingView)
		return;
	updatingView = true;

	if (object.IsConnected) {
		$("body").animate({ width: 430 }, { complete: function () {
			$("#start_chat").hide();
			$("#end_chat").show();
			$("#chat").show("fast", function () { updatingView = false; });
		}
		});
	}
	else {
		$("#chat").hide("fast", function () {
			$("#start_chat").show();
			$("#end_chat").hide();
			$("body").animate({ width: 220 }, { complete: function () { updatingView = false; } });
		});
	}

	$("#title").html(object.Name);

	$("#map").html(object.GetSetting("mapname"));

	$("#players").html("");
	var players;
	eval("players = " + object.GetPlayers() + ";");
	if (players) {
		for (var i = 0; i < players.length; i++) {
			$("#players").append("<div>" + players[i].Name + " - " + players[i].Frags + " - " + players[i].Ping + "</div>");
		} 
	}
}

$(document).ready(function () {
	object = Wrapper.GetServerInfo(object.Ip);

	if (!object.IsConnected) {
		$("body").width(220);
	}

	updateView();
	$("#start_chat").get(0).onclick = function () {
		object.IsConnected = true;
		updateView();
	};

	$("#end_chat").get(0).onclick = function () {
		object.IsConnected = false;
		updateView();
	};
	setInterval(updateView, 1000);
});