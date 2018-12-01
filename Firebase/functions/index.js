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

exports.reportUsage = functions.https.onRequest((request, response) => {
    let uid = request.body.uid;
    let lastRunTime = request.body.lastRunTime;
    let version = request.body.version;
    let nbPlugins = request.body.nbPlugins;

    let document
    if (uid === undefined) {
        // This is a new user!
        document = firestore.collection('usage').doc();
        document.set({ totalRuns: 0, runs: [] });
        uid = document.id
    }
    else {
        document = firestore.doc('usage/' + uid);
    }

    return document.get().then(documentSnapshot => {
        const currentCount = documentSnapshot.exists ? documentSnapshot.data().totalRuns : 0

        return document.set({
            totalRuns: Number(currentCount) + 1
        })
    })
        .then(() => { return document.collection('runs').add({ 'runtime': lastRunTime, 'version': version, 'nbPlugins': nbPlugins }) })
        .then(() => { return response.send(uid); })
});