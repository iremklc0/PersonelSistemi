var tumAPersoneller = [];
var tumBPersoneller = [];
Cesium.Ion.defaultAccessToken = 'eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJqdGkiOiI4MDMwMmJjMC0xZjg5LTQ4YWMtOWQzYy1mMzdkM2M3NzVhOTQiLCJpZCI6NDI2NTE4LCJpc3MiOiJodHRwczovL2lvbi5jZXNpdW0uY29tIiwiYXVkIjoidW5kZWZpbmVkX2RlZmF1bHQiLCJpYXQiOjE3Nzc3NTMyODR9.rox7zITjw86YNZcIZjCH2SgJ72bDFFqMlQV-QkNLABs';
var ankaraMerkez = ol.proj.fromLonLat([32.8597, 39.9208]);
var haritaGorunumu = new ol.View({ center: ankaraMerkez, zoom: 9, minZoom: 6, maxZoom: 18 });

var map = new ol.Map({
    target: 'map',
    layers: [new ol.layer.Tile({ source: new ol.source.OSM() })],
    view: haritaGorunumu,
    controls: ol.control.defaults.defaults({ attribution: false })
});
var ctrlIleDondur = new ol.interaction.DragRotate({
    condition: ol.events.condition.platformModifierKeyOnly
});
map.addInteraction(ctrlIleDondur);

var container = document.getElementById('popup');
var content = document.getElementById('popup-content');
var closer = document.getElementById('popup-closer');

var overlay = new ol.Overlay({
    element: container,
    autoPan: { animation: { duration: 250 } },
});
map.addOverlay(overlay);

closer.onclick = function () {
    overlay.setPosition(undefined);
    closer.blur();
    return false;
};

map.on('singleclick', function (evt) {
    if (typeof window.ol3d !== 'undefined' && window.ol3d.getEnabled()) return;
    var feature = map.forEachFeatureAtPixel(evt.pixel, function (f) { return f; });
    if (feature) {
        if (feature.get('koordinatPini')) {
            geciciPinSource.removeFeature(feature);
            document.getElementById('koordinat-popup').style.display = 'none';
            return;
        }
        if (feature.get('insaatBilgisi')) {
            var insaat = feature.get('insaatBilgisi');
            popupIcerikGoster(insaat, evt.coordinate);
        }
        if (feature.get('konteynerBilgisi')) {
            var konteyner = feature.get('konteynerBilgisi');
            konteynerPopupIcerikGoster(konteyner, evt.coordinate);
        }
    } else {
        overlay.setPosition(undefined);
    }
});

var fareSurukleniyorMu = false;

document.getElementById('map').addEventListener('pointerdown', function () {
    fareSurukleniyorMu = false;
});

document.getElementById('map').addEventListener('pointermove', function () {
    fareSurukleniyorMu = true;
});

document.getElementById('map').addEventListener('pointerup', function (e) {
    if (fareSurukleniyorMu || e.button !== 0) return;


    if (window.ol3d && window.ol3d.getEnabled()) {
        var cesiumScene = window.ol3d.getCesiumScene();
        if (!cesiumScene.globe.tilesLoaded) return;
        var rect = document.getElementById('map').getBoundingClientRect();
        var x = e.clientX - rect.left;
        var y = e.clientY - rect.top;
        var clickPozisyonu = new Cesium.Cartesian2(x, y);

        var pickedObject = cesiumScene.pick(clickPozisyonu);

        if (Cesium.defined(pickedObject) && pickedObject.primitive) {
            var feature = pickedObject.primitive.olFeature ||
                (pickedObject.id ? pickedObject.id.olFeature : null);


            if (feature && feature.get('koordinatPini')) {
                geciciPinSource.removeFeature(feature);
                document.getElementById('koordinat-popup').style.display = 'none';
                return;
            }
            if (feature && feature.get('insaatBilgisi')) {
                var insaat = feature.get('insaatBilgisi');
                var cartesian = cesiumScene.camera.pickEllipsoid(
                    clickPozisyonu,
                    cesiumScene.globe.ellipsoid
                );
                if (cartesian) {
                    var carto = Cesium.Cartographic.fromCartesian(cartesian);
                    var lon = Cesium.Math.toDegrees(carto.longitude);
                    var lat = Cesium.Math.toDegrees(carto.latitude);
                    var olKoordinat = ol.proj.fromLonLat([lon, lat]);
                    popupIcerikGoster(insaat, olKoordinat);
                }
            } else if (feature && feature.get('konteynerBilgisi')) {
                var konteyner = feature.get('konteynerBilgisi');
                var cartesian = cesiumScene.camera.pickEllipsoid(
                    clickPozisyonu,
                    cesiumScene.globe.ellipsoid
                );
                if (cartesian) {
                    var carto = Cesium.Cartographic.fromCartesian(cartesian);
                    var lon = Cesium.Math.toDegrees(carto.longitude);
                    var lat = Cesium.Math.toDegrees(carto.latitude);
                    var olKoordinat = ol.proj.fromLonLat([lon, lat]);
                    konteynerPopupIcerikGoster(konteyner, olKoordinat);
                }
            }
        } else {
            overlay.setPosition(undefined);
        }
    }
});




fetch('https://nominatim.openstreetmap.org/search?state=Ankara&country=Turkey&polygon_geojson=1&format=json')
    .then(response => response.json())
    .then(data => {
        if (data && data.length > 0) {
            var format = new ol.format.GeoJSON();
            var features = format.readFeatures(data[0].geojson, { dataProjection: 'EPSG:4326', featureProjection: 'EPSG:3857' });
            var sinir = new ol.layer.Vector({
                source: new ol.source.Vector({ features: features }),
                style: new ol.style.Style({ stroke: new ol.style.Stroke({ color: '#e74c3c', width: 4 }) })
            });
            map.addLayer(sinir);
        }
    });

var markerSource = new ol.source.Vector();
var markerLayer = new ol.layer.Vector({ source: markerSource });
map.addLayer(markerLayer);
var konteynerSource = new ol.source.Vector();
var konteynerLayer = new ol.layer.Vector({ source: konteynerSource });
map.addLayer(konteynerLayer);

function insaatlariHaritayaYukle() {
    fetch('/Harita/InsaatlariGetir')
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                markerSource.clear();


                var koordinatlar = [];
                data.data.forEach(function (insaat) {
                    var x = parseFloat(insaat.koordinatX);
                    var y = parseFloat(insaat.koordinatY);
                    if (!isNaN(x) && !isNaN(y)) {
                        koordinatlar.push(Cesium.Cartographic.fromDegrees(x, y));
                    }
                });


                var terrainProvider = Cesium.createWorldTerrain();
                var promise = Cesium.sampleTerrainMostDetailed(terrainProvider, koordinatlar);

                promise.then(function (guncelKoordinatlar) {

                    data.data.forEach(function (insaat, index) {
                        var x = parseFloat(insaat.koordinatX);
                        var y = parseFloat(insaat.koordinatY);

                        if (!isNaN(x) && !isNaN(y)) {
                            var yukseklik = guncelKoordinatlar[index].height;


                            var nokta = ol.proj.fromLonLat([x, y]);



                            var feature = new ol.Feature({
                                geometry: new ol.geom.Point(nokta),
                                insaatBilgisi: insaat
                            });


                            var catiRengi = '#f1c40f';
                            if (insaat.durumId === 0) catiRengi = '#e74c3c';
                            if (insaat.durumId === 2) catiRengi = '#2ecc71';

                            var svg3DBina = `
                                <svg width="56" height="64" viewBox="0 0 100 110" xmlns="http://www.w3.org/2000/svg">
                                    <polygon points="50,55 15,35 15,75 50,95" fill="#95a5a6" />
                                    <polygon points="50,55 85,35 85,75 50,95" fill="#ecf0f1" />
                                    <polygon points="50,15 15,35 50,55 85,35" fill="${catiRengi}" />
                                    <polygon points="25,48 35,54 35,64 25,58" fill="#2c3e50" />
                                    <polygon points="25,68 35,74 35,84 25,78" fill="#2c3e50" />
                                    <polygon points="75,48 65,54 65,64 75,58" fill="#bdc3c7" />
                                    <polygon points="75,68 65,74 65,84 75,78" fill="#bdc3c7" />
                                </svg>`;

                            var ikonUrl = 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(svg3DBina);

                            var stil = new ol.style.Style({
                                image: new ol.style.Icon({
                                    src: ikonUrl,
                                    scale: 1,
                                    anchor: [0.5, 0.9]
                                })
                            });

                            feature.setStyle(stil);


                            feature.set('olcs_altitudeMode', 'clampToGround');


                            markerSource.addFeature(feature);
                        }
                    });
                });
            }
        })
        .catch(error => console.error(error));
}
setTimeout(function () { insaatlariHaritayaYukle(); }, 300);



function tekInsaatGuncelle(insaatId, yeniDurumId) {
    var features = markerSource.getFeatures();
    var feature = features.find(function (f) {
        var bilgi = f.get('insaatBilgisi');
        return bilgi && bilgi.id === insaatId;
    });

    if (feature) {
        var insaat = feature.get('insaatBilgisi');
        insaat.durumId = parseInt(yeniDurumId);

        var catiRengi = '#f1c40f';
        if (insaat.durumId === 0) catiRengi = '#e74c3c';
        if (insaat.durumId === 2) catiRengi = '#2ecc71';

        var svg3DBina = '<svg width="56" height="64" viewBox="0 0 100 110" xmlns="http://www.w3.org/2000/svg"><polygon points="50,55 15,35 15,75 50,95" fill="#95a5a6" /><polygon points="50,55 85,35 85,75 50,95" fill="#ecf0f1" /><polygon points="50,15 15,35 50,55 85,35" fill="' + catiRengi + '" /><polygon points="25,48 35,54 35,64 25,58" fill="#2c3e50" /><polygon points="25,68 35,74 35,84 25,78" fill="#2c3e50" /><polygon points="75,48 65,54 65,64 75,58" fill="#bdc3c7" /><polygon points="75,68 65,74 65,84 75,78" fill="#bdc3c7" /></svg>';

        var ikonUrl = 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(svg3DBina);

        var yeniStil = new ol.style.Style({
            image: new ol.style.Icon({
                src: ikonUrl,
                scale: 1,
                anchor: [0.5, 0.9]
            })
        });

        feature.setStyle(yeniStil);
        feature.set('insaatBilgisi', insaat);
        overlay.setPosition(undefined);


        if (window.ol3d) {
            var sahne = window.ol3d.getCesiumScene();
            sahne.requestRender();

            if (window.ol3d && window.ol3d.getEnabled()) {
                var renderLoop = window.ol3d.getAutoRenderLoop ? window.ol3d.getAutoRenderLoop() : null;
                if (renderLoop && renderLoop.restartRenderLoop) {
                    setTimeout(function () { renderLoop.restartRenderLoop(); }, 100);
                }
            }
        }
    }
}

function tekInsaatSil(insaatId) {
    var features = markerSource.getFeatures();
    var feature = features.find(function (f) {
        var bilgi = f.get('insaatBilgisi');
        return bilgi && bilgi.id === insaatId;
    });

    if (feature) {
        markerSource.removeFeature(feature);
        overlay.setPosition(undefined);
    }
}

function popupIcerikGoster(insaat, koordinat) {
    
    content.innerHTML = '';

    var aPersonelListesi = insaat.aPersoneller || insaat.APersoneller || [];
    var bPersonelListesi = insaat.bPersoneller || insaat.BPersoneller || [];

    var aPersonelMetni = "";
    if (aPersonelListesi.length > 0) {
        aPersonelMetni = aPersonelListesi.map(function (p) {
            return '<span class="personel-etiket">👷 ' + p.adSoyad +
                ' <span class="cikar-btn" onclick="personelCikar(' + insaat.id + ', ' + p.id + ', \'A\')">×</span></span>';
        }).join("");
    }

    var bPersonelMetni = "";
    if (bPersonelListesi.length > 0) {
        bPersonelMetni = bPersonelListesi.map(function (p) {
            return '<span class="personel-etiket b-tipi">👷 ' + p.adSoyad +
                ' <span class="cikar-btn" onclick="personelCikar(' + insaat.id + ', ' + p.id + ', \'B\')">×</span></span>';
        }).join("");
    }

    content.innerHTML =
        '<h5 style="margin: 0 0 5px 0; color: #2c3e50;"><b>🏢 ' + insaat.insaatAdi + '</b></h5>' +
        '<hr style="margin: 5px 0;">' +
        '<p style="margin: 0; font-size: 14px;"><b>Türü:</b> ' + insaat.insaatTuru + '</p>' +
        '<p style="margin: 0; font-size: 14px;"><b>Açıklama:</b> ' + insaat.aciklama + '</p>' +
        (insaat.baslamaTarihi ? '<p style="margin: 0; font-size: 14px;"><b>Başlama Tarihi:</b> ' + new Date(insaat.baslamaTarihi).toLocaleDateString('tr-TR') + '</p>' : '') +

        (aPersonelMetni ? '<div class="personel-kutu">' +
            '<span class="personel-baslik">👷 A Sorumlu Personeller:</span>' +
            '<div>' + aPersonelMetni + '</div>' +
            '</div>' : '') +

        (bPersonelMetni ? '<div class="personel-kutu">' +
            '<span class="personel-baslik">👷 B Sorumlu Personeller:</span>' +
            '<div>' + bPersonelMetni + '</div>' +
            '</div>' : '') +

        '<div style="margin-top: 10px;">' +
        '<label style="font-size: 12px; font-weight: bold;">Durum Güncelle:</label>' +
        '<select onchange="durumDegistir(' + insaat.id + ', this.value)" style="width: 100%; padding: 5px; margin-top: 5px;">' +
        '<option value="0" ' + (insaat.durumId === 0 ? 'selected' : '') + '>🔴 Durduruldu</option>' +
        '<option value="1" ' + (insaat.durumId === 1 ? 'selected' : '') + '>🟡 Devam Ediyor</option>' +
        '<option value="2" ' + (insaat.durumId === 2 ? 'selected' : '') + '>🟢 Tamamlandı</option>' +
        '</select>' +
        '</div>' +
        (insaat.durumId === 1 ?
            '<div style="margin-top: 8px;">' +
            '<label style="font-size: 12px; font-weight: bold;">Tamamlanma Yüzdesi:</label>' +
            '<div style="display:flex; gap:5px; margin-top:5px;">' +
            '<input type="number" id="tamamlanmaYuzdesi" min="0" max="100" value="' + (insaat.tamamlanmaYuzdesi || 0) + '" style="width:70%; padding:5px;">' +
            '<button onclick="yuzdeyiGuncelle(' + insaat.id + ')" style="padding:5px 10px; background:#3498db; color:white; border:none; border-radius:4px; cursor:pointer;">💾</button>' +
            '</div>' +
            '</div>' : '') +

        '<div class="personel-kutu" style="margin-top:8px;">' +
        '<span class="personel-baslik">➕ A Personeli Ekle:</span>' +
        '<select id="popupAPersonelSelect" onchange="window.secilenAId=this.value" style="width:100%;padding:5px;margin-top:4px;border:1px solid #ddd;border-radius:4px;">' +
        '<option value="">Seçiniz...</option>' +
        tumAPersoneller.filter(function (p) {
            return !(insaat.aPersoneller || []).find(function (ap) { return ap.id === p.id; });
        }).map(function (p) { return '<option value="' + p.id + '">' + p.adSoyad + '</option>'; }).join('') +
        '</select>' +
        '<button onclick="personelEkle(' + insaat.id + ', window.secilenAId, \'A\'); window.secilenAId=\'\';" style="width:100%;margin-top:4px;padding:5px;background:#27ae60;color:white;border:none;border-radius:4px;cursor:pointer;">➕ Ekle</button>' +
        '</div>' +

        '<div class="personel-kutu" style="margin-top:8px;">' +
        '<span class="personel-baslik">➕ B Personeli Ekle:</span>' +
        '<select id="popupBPersonelSelect" onchange="window.secilenBId=this.value" style="width:100%;padding:5px;margin-top:4px;border:1px solid #ddd;border-radius:4px;">' +
        '<option value="">Seçiniz...</option>' +
        tumBPersoneller.filter(function (p) {
            return !(insaat.bPersoneller || []).find(function (bp) { return bp.id === p.id; });
        }).map(function (p) { return '<option value="' + p.id + '">' + p.adSoyad + '</option>'; }).join('') +
        '</select>' +
        '<button onclick="personelEkle(' + insaat.id + ', window.secilenBId, \'B\'); window.secilenBId=\'\';" style="width:100%;margin-top:4px;padding:5px;background:#27ae60;color:white;border:none;border-radius:4px;cursor:pointer;">➕ Ekle</button>' +
        '</div>' +

        '<button onclick="insaatSil(' + insaat.id + ')" class="button alert mini" style="width: 100%; margin-top: 10px;">' +
        '🗑️ İnşaatı Sil' +
        '</button>';

    overlay.setPosition(koordinat);

    setTimeout(function () {
        var btnA = document.getElementById('btnAEkle' + insaat.id);
        var btnB = document.getElementById('btnBEkle' + insaat.id);
        if (btnA) {
            btnA.addEventListener('click', function () {
                var v = document.getElementById('popupAPersonelSelect').value;
                personelEkle(insaat.id, v, 'A');
            });
        }
        if (btnB) {
            btnB.addEventListener('click', function () {
                var v = document.getElementById('popupBPersonelSelect').value;
                personelEkle(insaat.id, v, 'B');
            });
        }
    }, 100);
}
function konteynerPopupIcerikGoster(konteyner, koordinat) {
    var aPersonelListesi = konteyner.aPersoneller || [];
    var bPersonelListesi = konteyner.bPersoneller || [];

    var aPersonelMetni = "";
    if (aPersonelListesi.length > 0) {
        aPersonelMetni = aPersonelListesi.map(function (p) {
            return '<span class="personel-etiket">👷 ' + p.adSoyad +
                ' <span class="cikar-btn" onclick="konteynerPersonelCikar(' + konteyner.id + ', ' + p.id + ', \'A\')">×</span></span>';
        }).join("");
    }

    var bPersonelMetni = "";
    if (bPersonelListesi.length > 0) {
        bPersonelMetni = bPersonelListesi.map(function (p) {
            return '<span class="personel-etiket b-tipi">👷 ' + p.adSoyad +
                ' <span class="cikar-btn" onclick="konteynerPersonelCikar(' + konteyner.id + ', ' + p.id + ', \'B\')">×</span></span>';
        }).join("");
    }

    content.innerHTML =
        '<h5 style="margin: 0 0 5px 0; color: #2c3e50;"><b>🏠 ' + konteyner.ad + '</b></h5>' +
        '<hr style="margin: 5px 0;">' +

        (aPersonelMetni ? '<div class="personel-kutu">' +
            '<span class="personel-baslik">👷 A Sorumlu Personeller:</span>' +
            '<div>' + aPersonelMetni + '</div>' +
            '</div>' : '') +

        (bPersonelMetni ? '<div class="personel-kutu">' +
            '<span class="personel-baslik">👷 B Sorumlu Personeller:</span>' +
            '<div>' + bPersonelMetni + '</div>' +
            '</div>' : '') +

        '<div class="personel-kutu" style="margin-top:8px;">' +
        '<span class="personel-baslik">➕ A Personeli Ekle:</span>' +
        '<select id="popupKonteynerASelect" onchange="window.secilenKonteynerAId=this.value" style="width:100%;padding:5px;margin-top:4px;border:1px solid #ddd;border-radius:4px;">' +
        '<option value="">Seçiniz...</option>' +
        tumAPersoneller.filter(function (p) {
            return !aPersonelListesi.find(function (ap) { return ap.id === p.id; });
        }).map(function (p) { return '<option value="' + p.id + '">' + p.adSoyad + '</option>'; }).join('') +
        '</select>' +
        '<button onclick="konteynerPersonelEkle(' + konteyner.id + ', window.secilenKonteynerAId, \'A\');" style="width:100%;margin-top:4px;padding:5px;background:#27ae60;color:white;border:none;border-radius:4px;cursor:pointer;">➕ Ekle</button>' +
        '</div>' +

        '<div class="personel-kutu" style="margin-top:8px;">' +
        '<span class="personel-baslik">➕ B Personeli Ekle:</span>' +
        '<select id="popupKonteynerBSelect" onchange="window.secilenKonteynerBId=this.value" style="width:100%;padding:5px;margin-top:4px;border:1px solid #ddd;border-radius:4px;">' +
        '<option value="">Seçiniz...</option>' +
        tumBPersoneller.filter(function (p) {
            return !bPersonelListesi.find(function (bp) { return bp.id === p.id; });
        }).map(function (p) { return '<option value="' + p.id + '">' + p.adSoyad + '</option>'; }).join('') +
        '</select>' +
        '<button onclick="konteynerPersonelEkle(' + konteyner.id + ', window.secilenKonteynerBId, \'B\');" style="width:100%;margin-top:4px;padding:5px;background:#27ae60;color:white;border:none;border-radius:4px;cursor:pointer;">➕ Ekle</button>' +
        '</div>' +

        '<button onclick="konteynerSil(' + konteyner.id + ')" class="button alert mini" style="width: 100%; margin-top: 10px;">' +
        '🗑️ Konteyneri Sil' +
        '</button>';

    overlay.setPosition(koordinat);
}
function tekInsaatEkle(insaat) {
    var x = parseFloat(insaat.koordinatX);
    var y = parseFloat(insaat.koordinatY);

    if (isNaN(x) || isNaN(y)) return;

    var nokta = ol.proj.fromLonLat([x, y]);

    var feature = new ol.Feature({
        geometry: new ol.geom.Point(nokta),
        insaatBilgisi: insaat
    });

    var catiRengi = '#f1c40f';
    if (insaat.durumId === 0) catiRengi = '#e74c3c';
    if (insaat.durumId === 2) catiRengi = '#2ecc71';

    var svg3DBina = '<svg width="56" height="64" viewBox="0 0 100 110" xmlns="http://www.w3.org/2000/svg"><polygon points="50,55 15,35 15,75 50,95" fill="#95a5a6" /><polygon points="50,55 85,35 85,75 50,95" fill="#ecf0f1" /><polygon points="50,15 15,35 50,55 85,35" fill="' + catiRengi + '" /><polygon points="25,48 35,54 35,64 25,58" fill="#2c3e50" /><polygon points="25,68 35,74 35,84 25,78" fill="#2c3e50" /><polygon points="75,48 65,54 65,64 75,58" fill="#bdc3c7" /><polygon points="75,68 65,74 65,84 75,78" fill="#bdc3c7" /></svg>';

    var ikonUrl = 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(svg3DBina);

    var stil = new ol.style.Style({
        image: new ol.style.Icon({
            src: ikonUrl,
            scale: 1,
            anchor: [0.5, 0.9]
        })
    });

    feature.setStyle(stil);
    feature.set('olcs_altitudeMode', 'clampToGround');

    markerSource.addFeature(feature);
}
function tekKonteynerEkle(konteyner) {
    var lon = parseFloat(konteyner.boylam);
    var lat = parseFloat(konteyner.enlem);
    if (isNaN(lon) || isNaN(lat)) return;
    var nokta = ol.proj.fromLonLat([lon, lat]);
    var feature = new ol.Feature({
        geometry: new ol.geom.Point(nokta),
        konteynerBilgisi: konteyner
    });
    var svgKonteyner = `
    <svg width="56" height="56" viewBox="0 0 100 100" xmlns="http://www.w3.org/2000/svg">
        <rect x="10" y="40" width="80" height="50" fill="#3498db" rx="3"/>
        <polygon points="50,10 5,40 95,40" fill="#2980b9"/>
        <rect x="35" y="55" width="30" height="35" fill="#1a5276"/>
        <rect x="15" y="50" width="18" height="18" fill="#85c1e9"/>
        <rect x="67" y="50" width="18" height="18" fill="#85c1e9"/>
    </svg>`;

    var ikonUrl = 'data:image/svg+xml;charset=utf-8,' + encodeURIComponent(svgKonteyner);

    var stil = new ol.style.Style({
        image: new ol.style.Icon({
            src: ikonUrl,
            scale: 1,
            anchor: [0.5, 0.9]
        })
    });

    feature.setStyle(stil);
    feature.set('olcs_altitudeMode', 'clampToGround');
    konteynerSource.addFeature(feature);
}
function konteynerleriHaritayaYukle() {
    fetch('/Harita/KonteynerleriGetir')
        .then(response => response.json())
        .then(data => {
            if (data.success) {
                konteynerSource.clear();
                data.data.forEach(function (konteyner) {
                    tekKonteynerEkle(konteyner);
                });
            }
        })
        .catch(error => console.error(error));
}
setTimeout(function () { konteynerleriHaritayaYukle(); }, 600);

document.getElementById('btnAnaGorunum').addEventListener('click', function () {
    if (window.ol3d && window.ol3d.getEnabled()) {
        var cesiumCamera = window.ol3d.getCesiumScene().camera;
        cesiumCamera.flyTo({
            destination: Cesium.Cartesian3.fromDegrees(32.8597, 39.9208, 150000),
            orientation: {
                heading: Cesium.Math.toRadians(0),
                pitch: Cesium.Math.toRadians(-90),
                roll: 0.0
            },
            duration: 1.2
        });
    }

    else {
        haritaGorunumu.animate({
            center: ankaraMerkez,
            zoom: 9,
            rotation: 0,
            duration: 1200
        });
    }
});
window.CESIUM_BASE_URL = 'https://cesium.com/downloads/cesiumjs/releases/1.105/Build/Cesium/';
var btn2D3D = document.getElementById('btn2D3D');

window.addEventListener('load', function () {
    try {
        if (typeof olcs === 'undefined') {
            btn2D3D.title = '3D kullanılamıyor';
            btn2D3D.style.opacity = '0.5';
            return;
        }
        document.getElementById('btnKatmanYonetimi').addEventListener('click', function (e) {
            e.stopPropagation();
            var panel = document.getElementById('katmanPanel');
            panel.style.display = panel.style.display === 'none' ? 'block' : 'none';
        });

        document.addEventListener('click', function (e) {
            var panel = document.getElementById('katmanPanel');
            var btn = document.getElementById('btnKatmanYonetimi');
            if (!panel.contains(e.target) && e.target !== btn) {
                panel.style.display = 'none';
            }
        });

        document.getElementById('chkInsaatlar').addEventListener('change', function () {
            markerLayer.setVisible(this.checked);
        });

        document.getElementById('chkKonteynerler').addEventListener('change', function () {
            konteynerLayer.setVisible(this.checked);
        });

        btn2D3D.addEventListener('click', function () {

            if (!window.ol3d) {
                try {
                    window.ol3d = new olcs.OLCesium({ map: map });

                    var cesiumSahnesi = window.ol3d.getCesiumScene();


                    cesiumSahnesi.terrainProvider = new Cesium.EllipsoidTerrainProvider();


                    if (Cesium.createWorldTerrainAsync) {
                        Cesium.createWorldTerrainAsync().then(function (terrainProvider) {
                            cesiumSahnesi.terrainProvider = terrainProvider;
                        }).catch(function (e) {
                            console.warn('Terrain yüklenemedi:', e);
                        });
                    } else {
                        try {
                            cesiumSahnesi.terrainProvider = Cesium.createWorldTerrain();
                        } catch (e) {
                            console.warn('Terrain yüklenemedi:', e);
                        }
                    }


                    cesiumSahnesi.globe.depthTestAgainstTerrain = false;
                    cesiumSahnesi.globe.enableLighting = true;


                   

                } catch (e) {
                    console.error('3D başlatma hatası:', e);
                    Metro.notify.create('3D görünüm başlatılamadı: ' + e.message, 'Hata', { cls: 'alert' });
                    window.ol3d = null;
                    return;
                }
            }


            try {
                var suAn3Dmi = window.ol3d.getEnabled();

                if (suAn3Dmi) {
                    // 3D'den 2D'ye geç
                    window.ol3d.setEnabled(false);
                    overlay.setPosition(undefined);
                    btn2D3D.innerText = '3D';
                    btn2D3D.title = '3D Görünüme Geç';
                } else {
                    // 2D'den 3D'ye geç
                    window.ol3d.setEnabled(true);
                    btn2D3D.innerText = '2D';
                    btn2D3D.title = '2D Görünüme Geç';
                }
            } catch (e) {
                console.error('2D/3D toggle hatası:', e);
            }
        });

        var sagTikMenu = document.getElementById('sagTikMenu');
        var tiklananKoordinat = null;


        document.getElementById('map').addEventListener('contextmenu', function (e) {
            e.preventDefault();


            sagTikMenu.style.left = e.clientX + 'px';
            sagTikMenu.style.top = e.clientY + 'px';
            sagTikMenu.style.display = 'block';


            if (typeof window.ol3d !== 'undefined' && window.ol3d.getEnabled()) {

                var viewer = ol3d.getCesiumScene();
                var clickPozisyonu = new Cesium.Cartesian2(e.clientX, e.clientY);
                var cartesian = viewer.camera.pickEllipsoid(clickPozisyonu, viewer.globe.ellipsoid);

                if (cartesian) {
                    var cartographic = Cesium.Cartographic.fromCartesian(cartesian);
                    var lon = Cesium.Math.toDegrees(cartographic.longitude);
                    var lat = Cesium.Math.toDegrees(cartographic.latitude);
                    tiklananKoordinat = [lon, lat];
                }
            } else {

                var tiklananPiksel = map.getEventCoordinate(e);
                if (tiklananPiksel) {
                    tiklananKoordinat = ol.proj.toLonLat(tiklananPiksel);
                }
            }
        });

        document.addEventListener('pointerdown', function (e) {

            if (sagTikMenu.style.display === 'block' && !sagTikMenu.contains(e.target)) {
                sagTikMenu.style.display = 'none';
            }
        });



        document.getElementById('btnInsaatBaslat').addEventListener('click', function () {
            sagTikMenu.style.display = 'none';
            if (tiklananKoordinat) {
                var x = tiklananKoordinat[0].toFixed(6);
                var y = tiklananKoordinat[1].toFixed(6);

                document.getElementById('gizliKoordinatX').value = x.replaceAll('.', ',');
                document.getElementById('gizliKoordinatY').value = y.replaceAll('.', ',');
                document.getElementById('koordinatBilgiMetni').innerHTML = "📍 Seçilen Konum: X: " + x + " | Y: " + y;
                Metro.dialog.open('#insaatDialog');
            }
        });
        document.getElementById('btnKonteynerEkle').addEventListener('click', function () {
            sagTikMenu.style.display = 'none';
            if (tiklananKoordinat) {
                var x = tiklananKoordinat[0].toFixed(6);
                var y = tiklananKoordinat[1].toFixed(6);

                document.getElementById('konteynerKoordinatX').value = y;
                document.getElementById('konteynerKoordinatY').value = x;
                document.getElementById('konteynerKoordinatMetni').innerHTML = "📍 Seçilen Konum: Lon: " + x + " | Lat: " + y;
                Metro.dialog.open('#konteynerDialog');
            }
        });


        window.geciciPinSource = new ol.source.Vector();
        var geciciPinSource = window.geciciPinSource;

        var geciciPinLayer = new ol.layer.Vector({ source: window.geciciPinSource, zIndex: 999 });
        map.addLayer(geciciPinLayer);
        window.sonKoordinatMetni = '';

        //  KOORDİNAT EKLEME BUTONU
        document.getElementById('btnKoordinatEkle').addEventListener('click', function () {
            var sagTikMenu = document.getElementById('sagTikMenu');
            if (sagTikMenu) sagTikMenu.style.display = 'none';
            if (!tiklananKoordinat) return;

            var lon = tiklananKoordinat[0].toFixed(6);
            var lat = tiklananKoordinat[1].toFixed(6);
            window.sonKoordinatMetni = 'Lon: ' + lon + ' | Lat: ' + lat;

            var nokta = ol.proj.fromLonLat([parseFloat(lon), parseFloat(lat)]);
            var pinFeature = new ol.Feature({ geometry: new ol.geom.Point(nokta) });
            pinFeature.setStyle(new ol.style.Style({
                image: new ol.style.Circle({
                    radius: 8,
                    fill: new ol.style.Fill({ color: '#e74c3c' }),
                    stroke: new ol.style.Stroke({ color: 'white', width: 2 })
                })
            }));
            pinFeature.set('koordinatPini', true);
            window.geciciPinSource.addFeature(pinFeature);

            var pixel = map.getPixelFromCoordinate(nokta);
            var popup = document.getElementById('koordinat-popup');
            document.getElementById('koordinat-popup-icerik').innerHTML =
                '📍 <b>Koordinat</b><br>' +
                'Boylam: ' + lon + '<br>' +
                'Enlem: ' + lat;
            popup.style.left = (pixel[0] + 10) + 'px';
            popup.style.top = (pixel[1] - 80) + 'px';
            popup.style.display = 'block';
        });


        window.koordinatiKopyala = function () {
            navigator.clipboard.writeText(window.sonKoordinatMetni).then(function () {
                Metro.toast.create("Koordinat kopyalandı!", null, 1500, "bg-green fg-white");
            });
        };

        window.koordinatPopupuKapat = function () {
            var popup = document.getElementById('koordinat-popup');
            if (popup) popup.style.display = 'none';
        };
        document.getElementById('btnKaydet').addEventListener('click', function (e) {
            e.preventDefault();
            var formElemani = document.getElementById('frmInsaatEkle');
            var formVerileri = new FormData(formElemani);

           
            formVerileri.delete('secilenAPersoneller');
            formVerileri.delete('secilenBPersoneller');

            var aSelect = document.getElementById('aPersonelSelect');
            if (aSelect) {
                Array.from(aSelect.selectedOptions).forEach(function (opt) {
                    if (opt.value) formVerileri.append('secilenAPersoneller', opt.value);
                });
            }

            var bSelect = document.getElementById('bPersonelSelect');
            if (bSelect) {
                Array.from(bSelect.selectedOptions).forEach(function (opt) {
                    if (opt.value) formVerileri.append('secilenBPersoneller', opt.value);
                });
            }


            fetch('/Harita/InsaatEkle', {
                method: 'POST',
                body: formVerileri
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        Metro.dialog.close('#insaatDialog');
                        formElemani.reset();


                        if (aSelect) aSelect.selectedIndex = -1;
                        if (bSelect) bSelect.selectedIndex = -1;

                        var notify = Metro.notify.create("İnşaat başarıyla veritabanına kaydedildi!", "Başarılı", { cls: "success" });
                        setTimeout(function () { if (notify && notify.close) notify.close(); }, 3000);
                        tekInsaatEkle(data.data);
                    } else {
                        Metro.notify.create("Hata oluştu: " + data.message, "Hata", { cls: "alert" });
                    }
                })
                .catch(error => {
                    Metro.notify.create("Sunucuya bağlanılamadı.", "Bağlantı Hatası", { cls: "alert" });
                });
        });
        document.getElementById('btnKonteynerKaydet').addEventListener('click', function (e) {
            e.preventDefault();
            var formElemani = document.getElementById('frmKonteynerEkle');
            var formVerileri = new FormData(formElemani);

            formVerileri.delete('secilenAPersoneller');
            formVerileri.delete('secilenBPersoneller');

            var aSelect = document.getElementById('konteynerAPersonelSelect');
            if (aSelect) {
                Array.from(aSelect.selectedOptions).forEach(function (opt) {
                    if (opt.value) formVerileri.append('secilenAPersoneller', opt.value);
                });
            }

            var bSelect = document.getElementById('konteynerBPersonelSelect');
            if (bSelect) {
                Array.from(bSelect.selectedOptions).forEach(function (opt) {
                    if (opt.value) formVerileri.append('secilenBPersoneller', opt.value);
                });
            }

            fetch('/Harita/KonteynerEkle', {
                method: 'POST',
                body: formVerileri
            })
                .then(response => response.json())
                .then(data => {
                    if (data.success) {
                        Metro.dialog.close('#konteynerDialog');
                        formElemani.reset();
                        var notify = Metro.notify.create("Konteyner başarıyla kaydedildi!", "Başarılı", { cls: "success" });
                        setTimeout(function () { if (notify && notify.close) notify.close(); }, 3000);
                        tekKonteynerEkle(data.data);
                    } else {
                        Metro.notify.create("Hata: " + data.message, "Hata", { cls: "alert" });
                    }
                })
                .catch(error => {
                    Metro.notify.create("Sunucuya bağlanılamadı.", "Bağlantı Hatası", { cls: "alert" });
                });
        });

        window.durumDegistir = function (id, yeniId) {
            var formVerisi = new FormData();
            formVerisi.append("id", id);
            formVerisi.append("yeniDurumId", yeniId);

            fetch('/Harita/DurumGuncelle', { method: 'POST', body: formVerisi })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        var notify = Metro.notify.create("Durum güncellendi!", "Başarılı", { cls: "success" });
                        setTimeout(function () {
                            if (notify && notify.close) notify.close();
                        }, 3000);
                        tekInsaatGuncelle(id, yeniId);
                    }
                })
                .catch(err => console.error(err));
        };
        window.yuzdeyiGuncelle = function (id) {
            // DUPLIKASYON ÖNLEME
            if (window.yuzdeKilit) return;
            window.yuzdeKilit = true;
            setTimeout(function () { window.yuzdeKilit = false; }, 500);

          

          
            var tumInputlar = document.querySelectorAll('#tamamlanmaYuzdesi');
            var yuzde = '';
            var modu3D = window.ol3d && window.ol3d.getEnabled();

            for (var i = 0; i < tumInputlar.length; i++) {
                var inp = tumInputlar[i];
                var rect = inp.getBoundingClientRect();
                
                if (rect.width > 0 && rect.height > 0) {
                  
                    if (modu3D) {
                        yuzde = inp.value;
                    } else if (!yuzde) {
                        yuzde = inp.value;
                        break;
                    }
                }
            }

            var formVerisi = new FormData();
            formVerisi.append("id", id);
            formVerisi.append("yuzde", yuzde);
            fetch('/Harita/YuzdeGuncelle', { method: 'POST', body: formVerisi })
                .then(function (res) { return res.json(); })
                .then(function (data) {
                    if (data.success) {
                        var notify = Metro.notify.create("Yüzde güncellendi!", "Başarılı", { cls: "success" });
                        setTimeout(function () { if (notify && notify.close) notify.close(); }, 3000);
                        var feature = markerSource.getFeatures().find(function (f) {
                            return f.get('insaatBilgisi') && f.get('insaatBilgisi').id === id;
                        });
                        if (feature) {
                            feature.set('insaatBilgisi', data.data);
                            var geom = feature.getGeometry();
                            var koord = geom.getCoordinates();
                            popupIcerikGoster(data.data, koord);
                        }
                    } else {
                        Metro.notify.create("Hata: " + data.message, "Hata", { cls: "alert" });
                    }
                });
        };
        window.insaatSil = function (id) {
            if (window.silmeBekleniyor) return;
            window.silmeBekleniyor = true;
            setTimeout(function () { window.silmeBekleniyor = false; }, 1000);

            if (window.event) { window.event.preventDefault(); }
            if (!confirm("Bu inşaat kaydını tamamen silmek istediğinize emin misiniz?")) return;
            var formVerisi = new FormData();
            formVerisi.append("id", id);
            fetch('/Harita/InsaatSil', { method: 'POST', body: formVerisi })
                .then(res => {
                    if (!res.ok) throw new Error("Sunucu hatası");
                    return res.json();
                })
                .then(data => {
                    if (data.success) {
                        if (typeof overlay !== 'undefined') overlay.setPosition(undefined);
                        var notify = Metro.notify.create("Kayıt silindi.", "Bilgi", { cls: "info" }); setTimeout(function () { if (notify && notify.close) notify.close(); }, 3000);
                        tekInsaatSil(id);
                    } else {
                        Metro.notify.create("Veritabanından silinemedi.", "Hata", { cls: "alert" });
                    }
                })
                .catch(err => {
                    console.error(err);
                    Metro.notify.create("Sunucu bağlantı hatası.", "Hata", { cls: "alert" });
                });
        };
        window.personelCikar = function (insaatId, personelId, tip) {
            
            if (window.personelCikarKilit) return;
            window.personelCikarKilit = true;
            setTimeout(function () { window.personelCikarKilit = false; }, 1000);

            if (!confirm("Bu personeli inşaattan çıkarmak istediğinize emin misiniz?")) return;
            var formVerisi = new FormData();
            formVerisi.append("insaatId", insaatId);
            formVerisi.append("personelId", personelId);
            formVerisi.append("personelTipi", tip);

            fetch('/Harita/PersonelCikar', { method: 'POST', body: formVerisi })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        Metro.notify.create("Personel çıkarıldı", "Bilgi", { cls: "info" });
                        var feature = markerSource.getFeatures().find(function (f) {
                            var bilgi = f.get('insaatBilgisi');
                            return bilgi && bilgi.id === insaatId;
                        });
                        if (feature) {
                            feature.set('insaatBilgisi', data.data);
                            var geom = feature.getGeometry();
                            var koord = geom.getCoordinates();
                            popupIcerikGoster(data.data, koord);
                        }
                    } else {
                        Metro.notify.create("Hata: " + data.message, "Hata", { cls: "alert" });
                    }
                })
                .catch(function (err) {
                    console.error(err);
                    Metro.notify.create("Bağlantı hatası", "Hata", { cls: "alert" });
                });
        };
        window.personelEkle = function (insaatId, personelId, tip) {
            if (!personelId || personelId === '' || personelId === 'undefined') {
                return;
            }

            if (!personelId || personelId === '') {
                Metro.notify.create("Lütfen personel seçin!", "Uyarı", { cls: "warning" });
                return;
            }

            var formVerisi = new FormData();
            formVerisi.append("insaatId", insaatId);
            formVerisi.append("personelId", personelId);
            formVerisi.append("personelTipi", tip);

            fetch('/Harita/PersonelEkle', { method: 'POST', body: formVerisi })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        Metro.notify.create("Personel eklendi!", "Başarılı", { cls: "success" });
                        var feature = markerSource.getFeatures().find(function (f) {
                            var bilgi = f.get('insaatBilgisi');
                            return bilgi && bilgi.id === insaatId;
                        });
                        if (feature) {
                            feature.set('insaatBilgisi', data.data);
                            var geom = feature.getGeometry();
                            var koord = geom.getCoordinates();
                            popupIcerikGoster(data.data, koord);
                        }
                    } else {
                        Metro.notify.create("Hata: " + data.message, "Hata", { cls: "alert" });
                    }
                })
                .catch(function (err) {
                    console.error(err);
                    Metro.notify.create("Bağlantı hatası", "Hata", { cls: "alert" });
                });
        };
        window.konteynerSil = function (id) {
           
            if (window.konteynerSilKilit) return;
            window.konteynerSilKilit = true;
            setTimeout(function () { window.konteynerSilKilit = false; }, 1000);

            if (!confirm("Bu konteyneri silmek istediğinize emin misiniz?")) return;
            var formVerisi = new FormData();
            formVerisi.append("id", id);
            fetch('/Harita/KonteynerSil', { method: 'POST', body: formVerisi })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        overlay.setPosition(undefined);
                        var notify = Metro.notify.create("Konteyner silindi.", "Bilgi", { cls: "info" });
                        setTimeout(function () { if (notify && notify.close) notify.close(); }, 3000);
                        var feature = konteynerSource.getFeatures().find(function (f) {
                            return f.get('konteynerBilgisi') && f.get('konteynerBilgisi').id === id;
                        });
                        if (feature) konteynerSource.removeFeature(feature);
                    } else {
                        Metro.notify.create("Silinemedi: " + data.message, "Hata", { cls: "alert" });
                    }
                })
                .catch(err => Metro.notify.create("Bağlantı hatası", "Hata", { cls: "alert" }));
        };

        window.konteynerPersonelEkle = function (konteynerId, personelId, tip) {
            if (!personelId || personelId === '' || personelId === 'undefined') return;

            
            if (window.konteynerPersonelKilit) return;
            window.konteynerPersonelKilit = true;
            setTimeout(function () { window.konteynerPersonelKilit = false; }, 1000);

            var formVerisi = new FormData();
            formVerisi.append("konteynerId", konteynerId);
            formVerisi.append("personelId", personelId);
            formVerisi.append("personelTipi", tip);
            fetch('/Harita/KonteynerPersonelEkle', { method: 'POST', body: formVerisi })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        Metro.notify.create("Personel eklendi!", "Başarılı", { cls: "success" });
                        var feature = konteynerSource.getFeatures().find(function (f) {
                            return f.get('konteynerBilgisi') && f.get('konteynerBilgisi').id === konteynerId;
                        });
                        if (feature) {
                            feature.set('konteynerBilgisi', data.data);
                            konteynerPopupIcerikGoster(data.data, feature.getGeometry().getCoordinates());
                        }
                    } else {
                        Metro.notify.create("Hata: " + data.message, "Hata", { cls: "alert" });
                    }
                })
                .catch(err => Metro.notify.create("Bağlantı hatası", "Hata", { cls: "alert" }));
        };

        window.konteynerPersonelCikar = function (konteynerId, personelId, tip) {
            
            if (window.konteynerCikarKilit) return;
            window.konteynerCikarKilit = true;
            setTimeout(function () { window.konteynerCikarKilit = false; }, 1000);

            if (!confirm("Bu personeli konteynerden çıkarmak istediğinize emin misiniz?")) return;
            var formVerisi = new FormData();
            formVerisi.append("konteynerId", konteynerId);
            formVerisi.append("personelId", personelId);
            formVerisi.append("personelTipi", tip);
            fetch('/Harita/KonteynerPersonelCikar', { method: 'POST', body: formVerisi })
                .then(res => res.json())
                .then(data => {
                    if (data.success) {
                        Metro.notify.create("Personel çıkarıldı", "Bilgi", { cls: "info" });
                        var feature = konteynerSource.getFeatures().find(function (f) {
                            return f.get('konteynerBilgisi') && f.get('konteynerBilgisi').id === konteynerId;
                        });
                        if (feature) {
                            feature.set('konteynerBilgisi', data.data);
                            konteynerPopupIcerikGoster(data.data, feature.getGeometry().getCoordinates());
                        }
                    } else {
                        Metro.notify.create("Hata: " + data.message, "Hata", { cls: "alert" });
                    }
                })
                .catch(err => Metro.notify.create("Bağlantı hatası", "Hata", { cls: "alert" }));
        };
    } catch (e) {
        console.log(e);
    }
});

document.addEventListener("DOMContentLoaded", function () {
    fetch('/Harita/PersonelleriGetir')
        .then(function (res) { return res.json(); })
        .then(function (data) {
            if (data.success) {
                tumAPersoneller = data.aPersoneller || [];
                tumBPersoneller = data.bPersoneller || [];

                var aSelectEl = document.getElementById('aPersonelSelect');
                var bSelectEl = document.getElementById('bPersonelSelect');

                if (aSelectEl) {
                    aSelectEl.innerHTML = '<option value="">Seçiniz...</option>';
                    tumAPersoneller.forEach(function (p) {
                        var opt = document.createElement('option');
                        opt.value = p.id;
                        opt.textContent = p.adSoyad;
                        aSelectEl.appendChild(opt);
                    });
                    if (typeof Metro !== 'undefined') {
                        var plugin = Metro.getPlugin(aSelectEl, 'select');
                        if (plugin) plugin.reset();
                    }
                }

                if (bSelectEl) {
                    bSelectEl.innerHTML = '<option value="">Seçiniz...</option>';
                    tumBPersoneller.forEach(function (p) {
                        var opt = document.createElement('option');
                        opt.value = p.id;
                        opt.textContent = p.adSoyad;
                        bSelectEl.appendChild(opt);
                    });
                    if (typeof Metro !== 'undefined') {
                        var plugin = Metro.getPlugin(bSelectEl, 'select');
                        if (plugin) plugin.reset();
                    }
                }
                var konteynerASelectEl = document.getElementById('konteynerAPersonelSelect');
                if (konteynerASelectEl) {
                    konteynerASelectEl.innerHTML = '<option value="">Seçiniz...</option>';
                    tumAPersoneller.forEach(function (p) {
                        var opt = document.createElement('option');
                        opt.value = p.id;
                        opt.textContent = p.adSoyad;
                        konteynerASelectEl.appendChild(opt);
                    });
                }

                var konteynerBSelectEl = document.getElementById('konteynerBPersonelSelect');
                if (konteynerBSelectEl) {
                    konteynerBSelectEl.innerHTML = '<option value="">Seçiniz...</option>';
                    tumBPersoneller.forEach(function (p) {
                        var opt = document.createElement('option');
                        opt.value = p.id;
                        opt.textContent = p.adSoyad;
                        konteynerBSelectEl.appendChild(opt);
                    });
                }
            }
        })
        .catch(function (err) {
            console.error("Personeller yüklenirken hata:", err);
        });
});
