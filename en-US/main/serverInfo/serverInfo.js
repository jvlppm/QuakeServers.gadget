function updateServerInfo() {
	var info = Wrapper.GetServerInfo(object.Ip);
	eval("object = " + info + ";");
	updateView();
}

function updateView() {
	$("#title").html(object.Name);

	$("#map").html(object.Settings.mapname);

	$("#players").html("");
	for (var i = 0; i < object.Players.length; i++) {
		$("#players").append("<div>" + object.Players[i].Name + " - " + object.Players[i].Frags + " - " + object.Players[i].Ping + "</div>");
	}
}

$(document).ready(function () {
	updateView();
	setInterval(updateServerInfo, 1000);
});