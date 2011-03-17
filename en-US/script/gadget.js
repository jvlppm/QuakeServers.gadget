// Gadget

function SetupGadget() {
	System.Gadget.onDock = dockGadget;
	System.Gadget.onUndock = undockGadget;
	SetupError();
	checkDockState();
}

var afterErrorCallback;
function SetupError() {
	var errorDiv = $("#error");
	errorDiv.get(0).onclick = function () {
		errorDiv.hide("fast");
		if (afterErrorCallback) {
			afterErrorCallback();
			afterErrorCallback = null;
		}
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
			$(this).width($(this).parent().width() - $(this).css("left").replace("px", "") - $(this).css("right").replace("px", ""));
			$(this).height($(this).parent().height() - $(this).css("top").replace("px", "") - $(this).css("bottom").replace("px", ""));
		}
	});
}

//

var initialized = false;

function CheckForUpdates() {
	updater.CheckUpdates(System.Gadget.path, "en-US\\bin\\Quake2Client.dll", "Quake2Client.Quake2Client");
}

function CheckForLocalUpdates() {
	updater.CheckLocalUpdates(System.Gadget.path, "en-US\\bin\\Quake2Client.dll", "Quake2Client.Quake2Client");
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
	updater = builder.LoadType("Loader.dll", "Loader.GadgetLoader");
	if (!updater)
		throw { code: 1, message: "Loader não pode ser carregado"};
}

function Unregister() {
	builder.UnloadType(updater);
	updater = null;
	builder.UnregisterGadgetInterop();
	builder = null;
}

function ShowError(exception, hideErrorMessage, hideErrorMethod) {
	var text = exception;
	if (exception.message)
		text = exception.message;

	if (hideErrorMethod || !afterErrorCallback) {
		afterErrorCallback = hideErrorMethod;

		if (hideErrorMessage)
			$("#close_error").html(hideErrorMessage);
	}

	$("#error_message").html(text);
	$("#error").show("fast");
}

//

$(document).ready(function () {
	try {
		SetupGadget();
		SetupWrapper();
		CheckForUpdates();

		setInterval(updateGadget, 1000);
		updateGadget();
	}
	catch (Exception) {
		ShowError(Exception, "Tentar novamente", function () {
			document.URL = document.URL;
		});
	}
});