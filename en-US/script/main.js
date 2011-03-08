var checkForUpdatesMinutes = 5;

function UpdateView() {
	$("#main").html("");
	var servers = eval(Wrapper.Servers);
	for (var i = 0; i < servers.length; i++) {

	    if ((i % 2) == 0) {

	        $("#main").append('<div class="zebra_on">' + servers[i].Name + " -> " + servers[i].NumberOfPlayers +  'players</div>');

	    }else {

	        $("#main").append('<div class="zebra_off">' + servers[i].Name + " -> " + servers[i].NumberOfPlayers + 'players</div>');

        }
	}
}

$(document).ready(function () {
	UpdateView();
	Wrapper.UpdateServers();

	setInterval(UpdateView, 1000);
	setInterval(function () { Wrapper.UpdateServers(); }, 10000);

	setInterval(CheckForUpdates, checkForUpdatesMinutes * 60 * 1000);
});
