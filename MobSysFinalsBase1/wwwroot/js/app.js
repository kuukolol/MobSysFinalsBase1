window.playRingingSound = function() {
    var audio = new Audio('sounds/ringing.mp3');
    audio.loop = true;
    audio.play().catch(error => console.log("Error playing sound: ", error));
    window.ringingAudio = audio;
};

window.stopRingingSound = function() {
    if (window.ringingAudio) {
        window.ringingAudio.pause();
        window.ringingAudio.currentTime = 0;
    }
};
