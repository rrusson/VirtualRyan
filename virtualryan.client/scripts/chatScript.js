var $messages = $('.messages-content'),
	d, h, m,
	i = 0;

$(window).load(function () {
	$messages.mCustomScrollbar();
	setTimeout(function () {
		addBotResponse("Hi! I'm a bot that answers questions about Ryan Russon's resume and qualifications. What would you like to know?");
		$("#chatQuestion").focus();
	}, 100);
});

function updateScrollbar() {
	$messages.mCustomScrollbar("update").mCustomScrollbar('scrollTo', 'bottom', {
		scrollInertia: 10,
		timeout: 0
	});
}

function setDate() {
	d = new Date();
	if (m != d.getMinutes() || h != d.getHours()) {
		h = d.getHours();
		m = d.getMinutes();
		$('<div class="timestamp">' + String(h).padStart(2, '0') + ':' + String(m).padStart(2, '0') + '</div>').appendTo($('.message:last'));
	}
}

// Function to insert user message (called from React)
function insertUserMessage(message) {
	if ($.trim(message) == '') {
		return false;
	}

	$('<div class="message message-personal">' + message + '</div>').appendTo($('.mCSB_container')).addClass('new');
	setDate();
	$('.message-input').val(null);
	updateScrollbar();
}

// Function to add bot response (called from React)
function addBotResponse(response) {
	if ($.trim(response) == '') {
		return false;
	}
	$('<div class="message new"><figure class="avatar"><img src="https://avatars.githubusercontent.com/u/653188?v=4" /></figure>' + response + '</div>').appendTo($('.mCSB_container')).addClass('new');
	setDate();
	updateScrollbar();
}

function showBotLoading() {
	$('<div class="message loading new"><figure class="avatar"><img src="https://avatars.githubusercontent.com/u/653188?v=4" /></figure><span></span></div>').appendTo($('.mCSB_container'));
	updateScrollbar();
}

function hideBotLoading() {
	$('.message.loading').remove();
}

// Make functions globally accessible
window.insertUserMessage = insertUserMessage;
window.addBotResponse = addBotResponse;
window.showBotLoading = showBotLoading;
window.hideBotLoading = hideBotLoading;

$('.message-submit').click(function () {
	msg = $('.message-input').val();
	insertUserMessage(msg);
});
