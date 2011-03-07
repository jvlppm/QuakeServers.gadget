var builder;
var quake2;

$(document).ready(function () {
	builder = new GadgetBuilder();
	builder.Initialize();
	quake2 = builder.LoadType("Wrapper.dll", "Wrapper.Quake2Client");

	$("body").html(quake2.Id);
});

function Unregister() {
	builder.UnloadType(quake2);
	builder.UnregisterGadgetInterop();
	builder = null;
}
