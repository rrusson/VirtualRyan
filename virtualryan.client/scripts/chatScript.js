var $messages = $('.messages-content'),
	d, h, m,
	i = 0;

$(window).load(function () {
	$messages.mCustomScrollbar();
	setTimeout(function () {
		addBotResponse("Hi! I'm a bot that answers questions about Ryan's resume and qualifications. What would you like to know?");
	}, 100);
});

function updateScrollbar() {
	$messages.mCustomScrollbar("update").mCustomScrollbar('scrollTo', 'bottom', {
		scrollInertia: 10,
		timeout: 0
	});
}

function setDate() {
	d = new Date()
	if (m != d.getMinutes()) {
		m = d.getMinutes();
		$('<div class="timestamp">' + d.getHours() + ':' + m + '</div>').appendTo($('.message:last'));
	}
}

// Function to insert user message (called from React)
function insertUserMessage(message) {
	if ($.trim(message) == '') {
		return false;
	}
	$('<div class="message message-personal">' + message + '</div>').appendTo($('.mCSB_container')).addClass('new');
	setDate();
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

// Function to show loading indicator
function showBotLoading() {
	$('<div class="message loading new"><figure class="avatar"><img src="https://avatars.githubusercontent.com/u/653188?v=4" /></figure><span></span></div>').appendTo($('.mCSB_container'));
	updateScrollbar();
}

// Function to hide loading indicator
function hideBotLoading() {
	$('.message.loading').remove();
}

// Make functions globally accessible
window.insertUserMessage = insertUserMessage;
window.addBotResponse = addBotResponse;
window.showBotLoading = showBotLoading;
window.hideBotLoading = hideBotLoading;

function insertMessage() {
	msg = $('.message-input').val();
	if ($.trim(msg) == '') {
		return false;
	}
	$('<div class="message message-personal">' + msg + '</div>').appendTo($('.mCSB_container')).addClass('new');
	setDate();
	$('.message-input').val(null);
	updateScrollbar();
	setTimeout(function () {
		fakeMessage();
	}, 1000 + (Math.random() * 20) * 100);
}

$('.message-submit').click(function () {
	insertMessage();
});

$(window).on('keydown', function (e) {
	if (e.which == 13) {
		insertMessage();
		return false;
	}
})

var Fake = [
	'Hi there, I\'m Fabio and you?',
	'Nice to meet you',
	'How are you?',
	'Not too bad, thanks',
	'What do you do?',
	'That\'s awesome',
	'Codepen is a nice place to stay',
	'I think you\'re a nice person',
	'Why do you think that?',
	'Can you explain?',
	'Anyway I\'ve gotta go now',
	'It was a pleasure chat with you',
	'Time to make a new codepen',
	'Bye',
	':)'
]

function fakeMessage() {
	if ($('.message-input').val() != '') {
		return false;
	}
	showBotLoading();

	setTimeout(function () {
		hideBotLoading();
		$('<div class="message new"><figure class="avatar"><img src="https://avatars.githubusercontent.com/u/653188?v=4" /></figure>' + Fake[i] + '</div>').appendTo($('.mCSB_container')).addClass('new');
		setDate();
		updateScrollbar();
		i++;
	}, 1000 + (Math.random() * 20) * 100);
}