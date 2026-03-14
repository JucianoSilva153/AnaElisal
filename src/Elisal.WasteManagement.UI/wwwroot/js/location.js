window.locationTracker = {
    watchId: null,

    startTracking: function (dotNetRef) {
        if (navigator.geolocation) {
            this.watchId = navigator.geolocation.watchPosition(
                function (position) {
                    dotNetRef.invokeMethodAsync('OnLocationChanged', position.coords.latitude, position.coords.longitude);
                },
                function (error) {
                    console.error("Error getting location: ", error);
                },
                {
                    enableHighAccuracy: true,
                    timeout: 10000,
                    maximumAge: 0
                }
            );
        } else {
            console.error("Geolocation is not supported by this browser.");
        }
    },

    stopTracking: function () {
        if (this.watchId !== null && navigator.geolocation) {
            navigator.geolocation.clearWatch(this.watchId);
            this.watchId = null;
        }
    }
};
