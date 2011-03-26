var updatingView = false;

function updateView() {
	if (updatingView)
		return;
	updatingView = true;
	var lastMessage = $("#chat_text div").last();
	var autoScroll = $("#chat_text").scrollTop() >= ($("#chat_text").attr("scrollHeight") - $("#chat_text").height()) - lastMessage.height();
	lastMessage = lastMessage.html();

	$("#chat_text").html("");
	var messages;
	eval("messages = " + object.LastMessages + ";");
	for (var i = 0; i < messages.length; i++) {
		$("#chat_text").append("<div class='serverprint_" + messages[i].Type + "'>" + messages[i].Message.replace("\n", "") + "</div>");
	}
	if (messages.length > 0 && lastMessage != messages[messages.length - 1].Message.replace("\n", "")) {
		if (autoScroll) {
			$("#chat_text").animate({ scrollTop: $("#chat_text").attr("scrollHeight") }, 300);
		}
	}

	if (object.IsConneting || object.IsConnected) {
		$("body").animate({ width: 430 }, { complete: function () {
			$("#chat").show("fast", function () { updatingView = false; });
		}});
	}
	else {
		$("#chat").hide("fast", function () {
			$("body").animate({ width: 220 }, { complete: function () { updatingView = false; } });
		});
	}
	if (object.UpdatingConnection) {
		$("#start_chat").hide();
		$("#end_chat").hide();
		$("#chat_inp").hide();
	}
	else {
		if (object.IsConnected) {
			$("#start_chat").hide();
			$("#end_chat").show();
		}
		else {
			$("#start_chat").show();
			$("#end_chat").hide();
		}
	}

	if (Wrapper.GamePath && !Wrapper.IsPlaying)
		$("#join").show();
	else $("#join").hide();

	$("#title").html(object.Name);

	$("#map").html(object.GetSetting("mapname"));

	if (object.LastError) {
		$("#players").html("").hide();
		$("#server_error").html(object.LastError).show();
	}
	else {
		$("#players").html("").show();
		$("#server_error").html("").hide();

		var players;
		eval("players = " + object.GetPlayers() + ";");
		if (players) {
			for (var i = 0; i < players.length; i++) {
				$("#players").append("<div>" + players[i].Name + " - " + players[i].Frags + " - " + players[i].Ping + "</div>");
			}
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
		Wrapper.LaunchGame(object.Ip);
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