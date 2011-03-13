var updatingView = false;

function updateView() {
	if (updatingView)
		return;
	updatingView = true;
	$("#chat_text").html("");
	var messages;
	eval("messages = " + object.LastMessages + ";");
	for (var i = 0; i < messages.length; i++) {
		$("#chat_text").append("<div class='serverprint_" + messages[i].Level + "'>" + messages[i].Message + "</div>");
	}

	$("#chat_text").animate({ scrollTop: $("#chat_text").attr("scrollHeight") }, 300);

	if (object.UpdatingConnection) {
		$("#start_chat").hide();
		$("#end_chat").hide();
		$("#chat_inp").hide();
		$("#chat").hide("fast", function () { updatingView = false; });
	}
	else {
		if (object.IsConnected) {
			$("#chat_inp").show();
			$("body").animate({ width: 430 }, { complete: function () {
				$("#start_chat").hide();
				$("#end_chat").show();
				$("#chat").show("fast", function () { updatingView = false; });
			}});
		}
		else {
			$("#chat").hide("fast", function () {
				$("#start_chat").show();
				$("#end_chat").hide();
				$("body").animate({ width: 220 }, { complete: function () { updatingView = false; } });
			});
		}
	}

	if (Wrapper.GamePath && !object.IsPlaying)
		$("#join").show();
	else $("#join").hide();

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

	$("#chat_say").get(0).onclick = function () {
		object.Say($("#chat_input").val());
		$("#chat_input").val("");
	};

	$("#join").get(0).onclick = function () {
		$("#end_chat").get(0).onclick();
		Wrapper.LaunchGame(object);
	};

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