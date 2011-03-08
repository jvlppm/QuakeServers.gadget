// Gadget

function SetupGadget() {
	System.Gadget.onDock = dockGadget;
	System.Gadget.onUndock = undockGadget;
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

	$("*").each(function () {
		if ($(this).parent().is("body")) {
			$(this).width(width - $(this).css("left").replace("px", "") - $(this).css("right").replace("px", ""));
			$(this).height(height - $(this).css("top").replace("px", "") - $(this).css("bottom").replace("px", ""));
		}
		else {
			if ($(this).is("iframe")) {
				var a = 1 + 1;
			}

			$(this).width($(this).parent().width() - $(this).css("left").replace("px", "") - $(this).css("right").replace("px", ""));
			$(this).height($(this).parent().height() - $(this).css("top").replace("px", "") - $(this).css("bottom").replace("px", ""));
		}
	});
}

//

var initialized = false;
//var dllLocation = "%USERPROFILE%\\AppData\\Local\\Microsoft\\Windows Sidebar\\Gadgets\\QuakeServers.gadget";
var className = "Quake2Client.Quake2Client";

function CheckForUpdates() {
	var dllLocation = System.Gadget.path + "\\Quake2Client\\bin\\Debug\\Quake2Client.dll";
	updater.Load(dllLocation, className);
}

function updateGadget() {
	if (updater.CanUpdate || (!initialized && updater.WrapperLoaded)) {
		try {
			initialized = true;
			var wrapper = updater.Update();
			var newIframe = $("<iframe class=\"docked\" scrolling=\"no\" src=\"" + wrapper.Href + "\"></iframe>");

			$("#main").html("");
			$("#main").append(newIframe);
			newIframe.get(0).contentWindow.Wrapper = wrapper;
			newIframe.get(0).contentWindow.CheckForUpdates = CheckForUpdates;
			newIframe.get(0).contentWindow.ShowError = ShowError;
			newIframe.get(0).contentWindow.System = System;

			checkDockState();
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

$(document).ready(function () {
	SetupGadget();
	SetupWrapper();
	CheckForUpdates();

	setInterval(updateGadget, 1000);
});