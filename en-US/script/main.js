var checkForUpdatesMinutes = 5;

$(document).ready(function () {
	$("#main").html(Wrapper.Href);
	setInterval(CheckForUpdates, checkForUpdatesMinutes * 60 * 1000);
});
