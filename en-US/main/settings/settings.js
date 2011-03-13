var wrapper;

function UpdateView() {
	$("#gamepath").html(wrapper.LatchedGamePath);
	$("#gamecfg").html(wrapper.LatchedGameCFG);

	if (!$("#gamepath").html()) {
		$("#gamepath").html("&lt;Não selecionado&gt;");
		$("#gamepath").addClass("error");
	}
	else {
		$("#gamepath").removeClass("error");
	}

	if (!$("#gamecfg").html()) {
		$("#gamecfg").html("&lt;Não selecionado&gt;");
		$("#gamecfg").addClass("error");
	}
	else {
		$("#gamecfg").removeClass("error");
	}
}

$(document).ready(function () {
	wrapper = System.Gadget.document.Wrapper;
	UpdateView();

	$("#choose_gamepath").get(0).onclick = function () {
		wrapper.BrowseGamePath();
	};

	$("#choose_gamecfg").get(0).onclick = function () {
		wrapper.BrowseGameCFG();
	};

	setInterval(function () {
		UpdateView();
	}, 200);

	System.Gadget.onSettingsClosing = function (event) {
		if (event.closeAction == event.Action.commit) {
			wrapper.SaveSettings();
		}
		else if (event.closeAction == event.Action.cancel) {
			wrapper.DiscardSettings();
		}
	};
});