const API_KEY = "...";
const API_SECRET = "...";

console.log("Checking system requirements...");
console.log(JSON.stringify(ZoomMtg.checkSystemRequirements()));

console.log("Initializing Zoom...");
ZoomMtg.setZoomJSLib('https://source.zoom.us/1.8.1/lib', '/av');
ZoomMtg.preLoadWasm();
ZoomMtg.prepareJssdk();

const config = {
    meetingNumber: 123456,
    leaveUrl: 'https://google.ch',
    userName: 'Firstname Lastname',
    userEmail: 'firstname.lastname@yoursite.com',
    /* passWord: 'password', // if required */
    role: 0 // 1 for host; 0 for attendee
};

const signature = ZoomMtg.generateSignature({
    meetingNumber: config.meetingNumber,
    apiKey: API_KEY,
    apiSecret: API_SECRET,
    role: config.role,
    error: function (res) {
        console.error("FAILED TO GENERATE SIGNATURE: " + res)
    },
    success: function (res) {
        console.log("Successfully generated signature.");
        console.log(res.result);
    },
});

console.log("Initializing meeting...");

// See documentation: https://zoom.github.io/sample-app-web/ZoomMtg.html#init
ZoomMtg.init({
    debug: true, //optional
    leaveUrl: config.leaveUrl, //required
    // webEndpoint: 'PSO web domain', // PSO option
    showMeetingHeader: true, //option
    disableInvite: false, //optional
    disableCallOut: false, //optional
    disableRecord: false, //optional
    disableJoinAudio: false, //optional
    audioPanelAlwaysOpen: true, //optional
    showPureSharingContent: false, //optional
    isSupportAV: true, //optional,
    isSupportChat: false, //optional,
    isSupportQA: true, //optional,
    isSupportCC: true, //optional,
    screenShare: true, //optional,
    rwcBackup: '', //optional,
    videoDrag: true, //optional,
    sharingMode: 'both', //optional,
    videoHeader: true, //optional,
    isLockBottom: true, // optional,
    isSupportNonverbal: true, // optional,
    isShowJoiningErrorDialog: true, // optional,
    inviteUrlFormat: '', // optional
    loginWindow: {  // optional,
      width: 400,
      height: 380
    },
    // meetingInfo: [ // optional
    //   'topic',
    //   'host',
    //   'mn',
    //   'pwd',
    //   'telPwd',
    //   'invite',
    //   'participant',
    //   'dc'
    // ],
    disableVoIP: false, // optional
    disableReport: false, // optional
    error: function(res) {
        console.warn("INIT ERROR")
        console.log(res)
    },
    success: function() {
        ZoomMtg.join({
            signature: signature,
            apiKey: API_KEY,
            meetingNumber: config.meetingNumber,
            userName: config.userName,
            /* passWord: meetConfig.passWord, */
            error(res) {
                console.warn("JOIN ERROR")
                console.log(res)
            }
        })
    }
})
