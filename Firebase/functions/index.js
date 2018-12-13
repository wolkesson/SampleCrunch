// The Cloud Functions for Firebase SDK to create Cloud Functions and setup triggers.
const functions = require('firebase-functions');

// The Firebase Admin SDK to access the Database.
const admin = require('firebase-admin');
admin.initializeApp();

const firestore = admin.firestore();
// const settings = { timestampsInSnapshots: true};
// firestore.settings(settings);

exports.echo = functions.https.onRequest((request, response) => {
    response.send(request.body.text);
});

exports.registerUser = functions.https.onRequest((request, response) => {
    const collection = firestore.collection('users');

    // Use auto-ID if not specified otherwise. 
    let overrideUID = request.body.uid; // Used for testing
    let document = overrideUID ? collection.doc(overrideUID): collection.doc();

    return document.set({ 
        totalRuns: 0,
        totalTime: 0,
        version: request.body.version, 
        country: request.body.locale 
    })
    .then(() => { return response.send(document.id); })
});


exports.reportUsage = functions.https.onRequest((request, response) => {
    let uid = request.body.uid;
    let lastRunTime = request.body.lastRunTime | 0;

    let document = firestore.doc('users/' + uid);

    return document.get()
        .then(snap => {
            const runs = snap.exists ? snap.data().totalRuns : 0
            const time = snap.exists ? snap.data().totalTime : 0

            return document.update({
                totalRuns: Number(runs) + 1,
                totalTime: Number(time) + lastRunTime,
                // version: request.body.version
            })
        })
        .then(() => {
            return document.collection('runs').add({
                'runtime': lastRunTime,
                'parsers': request.body.numberOfParsers,
                'panels': request.body.numberOfPanels,
                'analyzers': request.body.numberOfAnalyzers
            })
        })
        .then(() => { return response.send(uid); })
});

exports.reportEvent = functions.https.onRequest((request, response) => {
    let uid = request.body.uid;
    let document = firestore.doc('users/' + uid);
    return document.get()
        .then(snap => { return document.collection('events').add(request.body) })
});

exports.reportError = functions.https.onRequest((request, response) => {
    return firestore.collection('errors').add(request.body);
});