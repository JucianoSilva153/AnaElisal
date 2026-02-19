window.mapFunctions = {
    maps: {},
    markers: {},
    routes: {},
    helpers: {},

    initMap: function (mapId, lat, lng, zoom) {
        if (this.maps[mapId]) {
            this.maps[mapId].remove();
        }

        const map = L.map(mapId).setView([lat, lng], zoom);
        L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
            maxZoom: 19,
            attribution: '© OpenStreetMap'
        }).addTo(map);

        map.on('click', function (e) {
            if (window.mapFunctions.helpers[mapId]) {
                try {
                    window.mapFunctions.helpers[mapId].invokeMethodAsync('OnMapClickInternal', e.latlng.lat, e.latlng.lng);
                } catch (err) {
                    // Componente não suporta click no mapa (ex: NovaRotaModal)
                }
            }
        });

        this.maps[mapId] = map;
        this.markers[mapId] = [];
        this.routes[mapId] = null;
        return map;
    },

    setHelper: function (mapId, helper) {
        this.helpers[mapId] = helper;
    },

    addMarkers: function (mapId, points) {
        const map = this.maps[mapId];
        if (!map) return;

        points.forEach(p => {
            const marker = L.marker([p.latitude, p.longitude])
                .addTo(map)
                .bindPopup(`<b>${p.name}</b><br>${p.address}`);

            if (!this.markers[mapId]) this.markers[mapId] = [];
            this.markers[mapId].push({ id: p.id, marker: marker });
        });
    },

    addSelectableMarkers: function (mapId, points, selectedIds) {
        const map = this.maps[mapId];
        if (!map) return;

        this.clearMarkers(mapId);

        points.forEach(p => {
            const isSelected = selectedIds && selectedIds.includes(p.id);

            // Custom icon for selected markers
            const icon = L.divIcon({
                className: 'custom-marker',
                html: `<div style="
                    background-color: ${isSelected ? '#10b981' : '#3b82f6'};
                    width: 30px;
                    height: 30px;
                    border-radius: 50%;
                    border: 3px solid white;
                    box-shadow: 0 2px 8px rgba(0,0,0,0.3);
                    display: flex;
                    align-items: center;
                    justify-content: center;
                    color: white;
                    font-weight: bold;
                    font-size: 12px;
                "></div>`,
                iconSize: [30, 30],
                iconAnchor: [15, 15]
            });

            const marker = L.marker([p.latitude, p.longitude], { icon: icon })
                .addTo(map)
                .bindPopup(`<b>${p.name}</b><br>${p.address}`);

            marker.on('click', () => {
                if (this.helpers[mapId]) {
                    this.helpers[mapId].invokeMethodAsync('OnPointSelected', p.id);
                }
            });

            if (!this.markers[mapId]) this.markers[mapId] = [];
            this.markers[mapId].push({ id: p.id, marker: marker });
        });
    },

    drawRoute: function (mapId, points) {
        const map = this.maps[mapId];
        if (!map) return;

        // Remove existing route
        if (this.routes[mapId]) {
            map.removeLayer(this.routes[mapId]);
        }

        if (!points || points.length < 2) return;

        // Create polyline
        const latlngs = points.map(p => [p.latitude, p.longitude]);
        const polyline = L.polyline(latlngs, {
            color: '#3b82f6',
            weight: 4,
            opacity: 0.7,
            smoothFactor: 1
        }).addTo(map);

        this.routes[mapId] = polyline;

        // Fit bounds to show entire route
        map.fitBounds(polyline.getBounds(), { padding: [50, 50] });
    },

    updateRouteWithOrder: function (mapId, points, selectedIds) {
        const map = this.maps[mapId];
        if (!map) return;

        // Update markers to show selection
        this.addSelectableMarkers(mapId, points, selectedIds);

        // Draw route for selected points in order
        if (selectedIds && selectedIds.length > 0) {
            const orderedPoints = selectedIds
                .map(id => points.find(p => p.id === id))
                .filter(p => p != null);
            this.drawRoute(mapId, orderedPoints);
        } else {
            // Clear route if no points selected
            if (this.routes[mapId]) {
                map.removeLayer(this.routes[mapId]);
                this.routes[mapId] = null;
            }
        }
    },

    clearMarkers: function (mapId) {
        const map = this.maps[mapId];
        if (!map) return;

        if (this.markers[mapId]) {
            this.markers[mapId].forEach(m => map.removeLayer(m.marker));
            this.markers[mapId] = [];
        }
    },

    clearRoute: function (mapId) {
        const map = this.maps[mapId];
        if (map && this.routes[mapId]) {
            map.removeLayer(this.routes[mapId]);
            this.routes[mapId] = null;
        }
    },

    destroy: function (mapId) {
        if (this.maps[mapId]) {
            this.maps[mapId].remove();
            delete this.maps[mapId];
            delete this.markers[mapId];
            delete this.routes[mapId];
            delete this.helpers[mapId];
        }
    }
};
