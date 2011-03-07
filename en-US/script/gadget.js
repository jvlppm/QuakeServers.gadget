var builder;
var quake2;

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

function SetupWrapper() {
	builder = new GadgetBuilder();
	builder.Initialize();
	quake2 = builder.LoadType("Wrapper.dll", "Wrapper.Quake2Client");
}

function Unregister() {
	builder.UnloadType(quake2);
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

	$("#main").html(quake2.Id);

	setTimeout(function () { ShowError("Teste"); }, 1000);
});
