// Gadget

function SetupGadget() {
	System.Gadget.onDock = dockGadget;
	SetupError();
	checkDockState();
}

function SetupError() {
	var errorDiv = $("#error");
	errorDiv.get(0).onclick = function () {
		errorDiv.hide("fast");
	};
}

function checkDockState() {
	if (System.Gadget.docked) {
		dockGadget();
	}
	else {
		undockGadget();
	}
}

function dockGadget() {
	updateSize(130, 205);
}

function undockGadget() {
	updateSize(445, 430);
}

function updateSize(width, height) {
	$("body").width(width);
	$("body").height(height);

	$("div").each(function () {
		if ($(this).parent().is("body")) {
			$(this).width(width - $(this).css("left").replace("px", "") - $(this).css("right").replace("px", ""));
			$(this).height(height - $(this).css("top").replace("px", "") - $(this).css("bottom").replace("px", ""));
		}
		else {
			$(this).width($(this).parent().width() - $(this).css("left").replace("px", "") - $(this).css("right").replace("px", ""));
			$(this).height($(this).parent().height() - $(this).css("top").replace("px", "") - $(this).css("bottom").replace("px", ""));
		}
	});
}

//

function CheckForUpdates() {
	updater.Load(dllLocation, "Quake2Client.Quake2Client");
}

function updateGadget() {
	if (updater.CanUpdate) {
		try {
			var wrapper = updater.Update();
			var newIframe = $("<iframe scrolling=\"no\" src=\"" + wrapper.Href + "\"></iframe>");

			$("#main").html("");
			$("#main").append(newIframe);
			newIframe.get(0).contentWindow.Wrapper = wrapper;
			newIframe.get(0).contentWindow.CheckForUpdates = CheckForUpdates;
			newIframe.get(0).contentWindow.ShowError = ShowError;
		} catch (Exception) {
			ShowError(Exception);
		}
	}
}

//

var builder;
var updater;

function SetupWrapper() {
	builder = new GadgetBuilder();
	builder.Initialize();
	updater = builder.LoadType("Wrapper.dll", "Wrapper.GadgetLoader");
}

function Unregister() {
	builder.UnloadType(updater);
	updater = null;
	builder.UnregisterGadgetInterop();
	builder = null;
}

function ShowError(message) {
	$("#error").html(message).show("fast");
}

//
var dllLocation = "D:\\Documents\\Projects\\QuakeServers.gadget\\Quake2Client\\bin\\Debug\\Quake2Client.dll";

$(document).ready(function () {
	SetupGadget();
	SetupWrapper();
	CheckForUpdates();

	setInterval(updateGadget, 1000);
});