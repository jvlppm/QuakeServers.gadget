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

function CheckForUpdates() {
	updater.CheckUpdates(System.Gadget.path);
}

function CheckForLocalUpdates() {
	updater.CheckLocalUpdates(System.Gadget.path);
}

function LimitValue(value, min, max) {
	if (value < min)
		return min;
	if (value > max)
		return max;
	return value;
}

function updateGadget() {
	CheckForLocalUpdates();
	if (updater.CanUpdate || (!initialized && updater.WrapperLoaded)) {

		if (updater.LastUpdateTime)
			setInterval(CheckForUpdates, LimitValue(updater.LastUpdateTime, 0.05, 60) * 60 * 1000);

		try {
			initialized = true;
			var wrapper = updater.Update();
			var newIframe = $("<iframe class='docked' scrolling='no' src='main/main.html'></iframe>");

			$("#main").html("");
			$("#main").append(newIframe);
			try {
				newIframe.get(0).contentWindow.Wrapper = wrapper;
				newIframe.get(0).contentWindow.CheckForUpdates = CheckForUpdates;
				newIframe.get(0).contentWindow.ShowError = ShowError;
				newIframe.get(0).contentWindow.System = System;
			} catch (Exception) {
				document.location = document.location;
			}

			$("#error").hide("fast");

			checkDockState();
		} catch (Exception) {
			if (Exception.message)
				Exception = Exception.message;
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
	document.body.onclick = null;
	SetupGadget();
	SetupWrapper();
	CheckForUpdates();
	updateGadget();

	setInterval(updateGadget, 1000);
});