var zSayac = 100000;
var panelSabitMi = false;

function pencereOncePlan(winEl) {
    if ($(winEl).length > 0) { zSayac++; $(winEl).css('z-index', zSayac); }
}

$(document).on('mousedown', '.window', function () { pencereOncePlan(this); });
$(document).on('mouseenter', '.window', function () {
    var buZ = parseInt($(this).css('z-index')) || 0;
    var enUstZ = 0;
    $('.window').each(function () {
        var z = parseInt($(this).css('z-index')) || 0;
        if (z > enUstZ) enUstZ = z;
    });
    if (buZ < enUstZ) { $(this).find('.iframe-overlay').show(); }
});
$(document).on('mouseleave', '.window', function () { $(this).find('.iframe-overlay').hide(); });
$(document).on('mousedown', '.iframe-overlay', function () {
    pencereOncePlan($(this).closest('.window')[0]); $(this).hide();
});

$(document).ready(function () {
    var path = window.location.pathname;
    $('.app-bar-menu a, .app-bar-item').removeClass('aktif-menu');
    $('.app-bar-menu a, .app-bar-item').each(function () {
        var href = $(this).attr('href');
        if (href === path || (path === "/" && href === "/Home/Index")) {
            $(this).addClass('aktif-menu');
        }
    });

    var kTema = localStorage.getItem('otek_sistem_temasi');
    var kYazi = localStorage.getItem('otek_sistem_yazi');
    var kVurgu = localStorage.getItem('otek_sistem_vurgu');

    if (kTema) {
        document.documentElement.style.setProperty('--tema-arkaplan', kTema);
        document.documentElement.style.setProperty('--tema-yazi', kYazi);
        document.documentElement.style.setProperty('--tema-vurgu', kVurgu);
        $('#tema-renk-secici').val(kTema);
    }

    $('.menuler-tiklama').on('click', function () {
        $('.menuler-tiklama').removeClass('aktif-menu');
        $(this).addClass('aktif-menu');
    });
});

function getContrastColor(hexcolor) {
    hexcolor = hexcolor.replace("#", "");
    var r = parseInt(hexcolor.substr(0, 2), 16), g = parseInt(hexcolor.substr(2, 2), 16), b = parseInt(hexcolor.substr(4, 2), 16);
    var yiq = ((r * 299) + (g * 587) + (b * 114)) / 1000;
    return (yiq >= 128) ? '#000000' : '#ffffff';
}

function renkKoyulastir(hex, lum) {
    hex = String(hex).replace(/[^0-9a-f]/gi, '');
    if (hex.length < 6) hex = hex[0] + hex[0] + hex[1] + hex[1] + hex[2] + hex[2];
    lum = lum || 0;
    var rgb = "#", c, i;
    for (i = 0; i < 3; i++) {
        c = parseInt(hex.substr(i * 2, 2), 16);
        c = Math.round(Math.min(Math.max(0, c + (c * lum)), 255)).toString(16);
        rgb += ("00" + c).substr(c.length);
    }
    return rgb;
}

function iframeStilEnjekteEt(arkaplan, yazi, vurgu) {
    $('iframe').each(function () {
        try {
            var $head = $(this).contents().find('head');
            if ($head.length > 0) {
                $head.find('#dinamik-tema-stili').remove();
                var css = '<style id="dinamik-tema-stili">' +
                    'html body .button:not(.alert), html body .button.primary, html body .button.warning, html body .button.success, html body .button.secondary, html body .button.info { ' +
                    'background-color: ' + arkaplan + ' !important; border-color: ' + arkaplan + ' !important; color: ' + yazi + ' !important; box-shadow: none !important; outline: none !important; } ' +
                    'html body .button:not(.alert):hover { background-color: ' + vurgu + ' !important; border-color: ' + vurgu + ' !important; opacity: 1 !important; } ' +
                    'html body .button.alert { background-color: #ce352c !important; border-color: #ce352c !important; color: #ffffff !important; } ' +
                    'html body .button.alert:hover { background-color: #9a2620 !important; border-color: #9a2620 !important; }' +
                    '</style>';
                $head.append(css);
            }
        } catch (e) { }
    });
}

function iframeYuklendi(ifr) {
    var kTema = localStorage.getItem('otek_sistem_temasi') || '#343a40';
    var kYazi = localStorage.getItem('otek_sistem_yazi') || '#ffffff';
    var kVurgu = localStorage.getItem('otek_sistem_vurgu') || '#1d2124';
    iframeStilEnjekteEt(kTema, kYazi, kVurgu);
}

function temaDegistir(yeniRenk) {
    var yaziRengi = getContrastColor(yeniRenk);
    var vurguRengi = renkKoyulastir(yeniRenk, -0.15);
    document.documentElement.style.setProperty('--tema-arkaplan', yeniRenk);
    document.documentElement.style.setProperty('--tema-yazi', yaziRengi);
    document.documentElement.style.setProperty('--tema-vurgu', vurguRengi);
    localStorage.setItem('otek_sistem_temasi', yeniRenk);
    localStorage.setItem('otek_sistem_yazi', yaziRengi);
    localStorage.setItem('otek_sistem_vurgu', vurguRengi);
    iframeStilEnjekteEt(yeniRenk, yaziRengi, vurguRengi);
}

function temayiSifirla() {
    var varsayilanArka = '#343a40'; var varsayilanYazi = '#ffffff'; var varsayilanVurgu = '#1d2124';
    document.documentElement.style.setProperty('--tema-arkaplan', varsayilanArka);
    document.documentElement.style.setProperty('--tema-yazi', varsayilanYazi);
    document.documentElement.style.setProperty('--tema-vurgu', varsayilanVurgu);
    localStorage.removeItem('otek_sistem_temasi'); localStorage.removeItem('otek_sistem_yazi'); localStorage.removeItem('otek_sistem_vurgu');
    document.getElementById('tema-renk-secici').value = varsayilanArka;
    iframeStilEnjekteEt(varsayilanArka, varsayilanYazi, varsayilanVurgu);
}

function dosyaAc(url, resimMi, dosyaAdi) {
    if (resimMi) {
        document.getElementById('gorsel-overlay-img').src = url;
        document.getElementById('gorsel-overlay-sil').setAttribute('data-dosya', dosyaAdi);
        document.getElementById('gorsel-overlay').classList.add('acik');
        return;
    }
    var pencereIcerik = $('.win-belge .window-content');
    if (pencereIcerik.length === 0) return;
    var listeAlani = pencereIcerik.find('.belge-liste-alani');
    var onizlemeAlani = pencereIcerik.find('.belge-onizleme-alani');
    var baslik = pencereIcerik.find('.belge-onizleme-baslik');
    var icerik = pencereIcerik.find('.belge-onizleme-icerik');
    listeAlani.hide();
    onizlemeAlani.css('display', 'flex');
    baslik.text(dosyaAdi);
    icerik.html('<div class="text-center mt-5"><span class="mif-spinner2 ani-spin mif-4x fg-cyan"></span><br><br><b>Belge Hazırlanıyor...</b></div>');
    var uzanti = dosyaAdi.split('.').pop().toLowerCase();
    if (uzanti === 'pdf') {
        icerik.html('<iframe src="' + url + '#toolbar=1" style="width:100%; height:100%; min-height:400px; border:none;"></iframe>');
    } else if (uzanti === 'doc' || uzanti === 'docx') {
        fetch(url).then(function (r) { return r.arrayBuffer(); }).then(function (b) {
            mammoth.convertToHtml({ arrayBuffer: b }).then(function (res) { icerik.html(res.value); })
                .catch(function () { icerik.html("<h4 class='fg-red text-center mt-5'>Belge okunamadı.</h4>"); });
        });
    } else {
        icerik.html("<div class='text-center mt-5'><span class='mif-file-excel mif-5x fg-green mb-4 d-block'></span><h5>Bu format sadece indirilebilir.</h5><a href='" + url + "' download='" + dosyaAdi + "' class='button success mt-2'>Dosyayı İndir</a></div>");
    }
}

function onizlemeyiKapat(btn) {
    var pencereIcerik = $(btn).closest('.window-content');
    pencereIcerik.find('.belge-onizleme-alani').hide();
    pencereIcerik.find('.belge-onizleme-icerik').html('');
    pencereIcerik.find('.belge-liste-alani').show();
}

function gorselKapat() { document.getElementById('gorsel-overlay').classList.remove('acik'); document.getElementById('gorsel-overlay-img').src = ''; }
document.addEventListener('keydown', function (e) { if (e.key === 'Escape') gorselKapat(); });

function kutuAc(pencereBasligi, sayfaLinki) {
    var ozelSinif = "win-" + pencereBasligi.replace(/[^a-zA-Z0-9]/g, '-').toLowerCase();
    var mevcut = $('.' + ozelSinif); if (mevcut.length > 0) { pencereOncePlan(mevcut); return; }
    Metro.window.create({
        title: pencereBasligi, clsWindow: ozelSinif, shadow: true, draggable: true, resizable: true, maximizable: true, minimizable: true, closable: true,
        width: 900, height: 600, place: 'center', clsCaption: 'dinamik-tema-arkaplan',
        content: "<div class='iframe-kapsayici'><iframe src='" + sayfaLinki + "' onload='iframeYuklendi(this)'></iframe><div class='iframe-overlay'></div></div>",
        onDragStart: function (e, w) { $(w).find('iframe').css('pointer-events', 'none'); $(w).find('.iframe-overlay').show(); },
        onDragStop: function (e, w) { $(w).find('iframe').css('pointer-events', 'auto'); $(w).find('.iframe-overlay').hide(); },
        onResizeStart: function (e, w) { $(w).find('iframe').css('pointer-events', 'none'); $(w).find('.iframe-overlay').show(); },
        onResizeStop: function (e, w) { $(w).find('iframe').css('pointer-events', 'auto'); $(w).find('.iframe-overlay').hide(); $(w).find('.iframe-kapsayici').css('height', ($(w).height() - 40) + 'px'); },
        onWindowCreate: function (w) { $(w).on('mousedown', function () { pencereOncePlan(this); }); }
    });
}

function medyaPenceresiAc() {
    var mevcut = $('.win-medya'); if (mevcut.length > 0) { pencereOncePlan(mevcut); return; }
    var icerik = document.getElementById('medya-icerik-sablonu').innerHTML;
    Metro.window.create({ title: 'Medya Yönetimi', clsWindow: 'win-medya', shadow: true, draggable: true, resizable: true, maximizable: true, minimizable: true, closable: true, width: 950, height: 600, place: 'center', clsCaption: 'dinamik-tema-arkaplan', content: '<div style="padding:15px; max-height:560px; overflow-y:auto;">' + icerik + '</div>', onWindowCreate: function (w) { $(w).on('mousedown', function () { pencereOncePlan(this); }); } });
    setTimeout(function () { var fileInp = $('.win-medya').find('input[type="file"]')[0]; if (fileInp) Metro.makePlugin(fileInp, 'file', { buttonTitle: "Seç" }); }, 100);
}

function belgePenceresiAc() {
    var mevcut = $('.win-belge'); if (mevcut.length > 0) { pencereOncePlan(mevcut); return; }
    var icerik = document.getElementById('belge-icerik-sablonu').innerHTML;
    Metro.window.create({ title: 'Belge Yönetimi', clsWindow: 'win-belge', shadow: true, draggable: true, resizable: true, maximizable: true, minimizable: true, closable: true, width: 950, height: 600, place: 'center', clsCaption: 'dinamik-tema-arkaplan', content: '<div style="padding:15px; max-height:560px; overflow-y:auto;">' + icerik + '</div>', onWindowCreate: function (w) { $(w).on('mousedown', function () { pencereOncePlan(this); }); } });
    setTimeout(function () { var fileInp = $('.win-belge').find('input[type="file"]')[0]; if (fileInp) Metro.makePlugin(fileInp, 'file', { buttonTitle: "Seç" }); }, 100);
}

function insaatListesiAc() {
    var mevcut = $('.win-insaat-listesi');
    if (mevcut.length > 0) { pencereOncePlan(mevcut); return; }
    fetch('/Harita/InsaatlariGetir')
        .then(function (res) { return res.json(); })
        .then(function (data) {
            if (!data.success) return;
            var insaatlar = data.data;
            window._insaatListesiCache = {};
            insaatlar.forEach(function (i) { window._insaatListesiCache[i.id] = i; });
            var devamEdenler = insaatlar.filter(function (i) { return i.durumId === 1; });
            var durdurulmus = insaatlar.filter(function (i) { return i.durumId === 0; });
            var tamamlanan = insaatlar.filter(function (i) { return i.durumId === 2; });
            function grupHtml(baslik, renk, emoji, liste) {
                if (liste.length === 0) return '';
                var html = '<div style="margin-bottom:20px;">';
                html += '<h6 style="color:' + renk + '; border-bottom:2px solid ' + renk + '; padding-bottom:5px;">' + emoji + ' ' + baslik + ' (' + liste.length + ')</h6>';
                liste.forEach(function (i) {
                    html += '<div style="display:flex; justify-content:space-between; align-items:center; padding:8px; margin-bottom:5px; background:#f9f9f9; border-radius:6px; border-left:4px solid ' + renk + ';">';
                    html += '<div><b>' + i.insaatAdi + '</b>';
                    html += '<span style="font-size:12px; color:#666; margin-left:10px;">' + (i.insaatTuru || '') + '</span></div>';
                    html += '<button style="background:#3498db; color:white; border:none; border-radius:4px; padding:4px 10px; cursor:pointer;" onclick="insaatRaporuGosterById(' + i.id + ')">Insaat Durumu</button>';
                    html += '</div>';
                });
                html += '</div>';
                return html;
            }
            var icerik = '<div style="padding:15px; max-height:500px; overflow-y:auto;">';
            icerik += grupHtml('Devam Ediyor', '#f39c12', '🟡', devamEdenler);
            icerik += grupHtml('Durduruldu', '#e74c3c', '🔴', durdurulmus);
            icerik += grupHtml('Tamamlandi', '#27ae60', '🟢', tamamlanan);
            icerik += '</div>';
            Metro.window.create({
                title: 'Insaat Listesi', clsWindow: 'win-insaat-listesi',
                shadow: true, draggable: true, resizable: true,
                maximizable: true, minimizable: true, closable: true,
                width: 700, height: 550, place: 'center',
                clsCaption: 'dinamik-tema-arkaplan', content: icerik,
                onWindowCreate: function (w) { $(w).on('mousedown', function () { pencereOncePlan(this); }); }
            });
        });
}

function insaatRaporuGosterById(id) {
    var insaat = window._insaatListesiCache[id];
    if (insaat) insaatRaporuGoster(insaat);
}

function insaatRaporuGoster(insaat) {
    var aPersoneller = (insaat.aPersoneller || insaat.APersoneller || []).map(function (p) { return p.adSoyad; }).join(', ') || 'Yok';
    var bPersoneller = (insaat.bPersoneller || insaat.BPersoneller || []).map(function (p) { return p.adSoyad; }).join(', ') || 'Yok';
    var durum = insaat.durumId === 0 ? 'Durduruldu' : insaat.durumId === 1 ? 'Devam Ediyor' : 'Tamamlandi';
    var tarih = insaat.baslamaTarihi ? new Date(insaat.baslamaTarihi).toLocaleDateString('tr-TR') : 'Belirtilmemis';
    var icerik = '<div style="padding:20px; font-size:14px;">';
    icerik += '<h5 style="margin-bottom:15px;">' + insaat.insaatAdi + '</h5>';
    icerik += '<table style="width:100%; border-collapse:collapse;">';
    icerik += '<tr><td style="padding:8px; font-weight:bold; width:40%;">Tur:</td><td>' + (insaat.insaatTuru || '-') + '</td></tr>';
    icerik += '<tr style="background:#f9f9f9;"><td style="padding:8px; font-weight:bold;">Durum:</td><td>' + durum + '</td></tr>';
    icerik += '<tr><td style="padding:8px; font-weight:bold;">Aciklama:</td><td>' + (insaat.aciklama || '-') + '</td></tr>';
    icerik += '<tr style="background:#f9f9f9;"><td style="padding:8px; font-weight:bold;">Baslama Tarihi:</td><td>' + tarih + '</td></tr>';
    icerik += '<tr><td style="padding:8px; font-weight:bold;">Tamamlanma:</td><td>%' + (insaat.tamamlanmaYuzdesi || 0) + '</td></tr>';
    icerik += '<tr style="background:#f9f9f9;"><td style="padding:8px; font-weight:bold;">A Personelleri:</td><td>' + aPersoneller + '</td></tr>';
    icerik += '<tr><td style="padding:8px; font-weight:bold;">B Personelleri:</td><td>' + bPersoneller + '</td></tr>';
    icerik += '</table>';
    icerik += '<div style="text-align:right; margin-top:15px;">';
    icerik += '<button onclick="raporuYazdir()" style="padding:8px 16px; background:#2c3e50; color:white; border:none; border-radius:4px; cursor:pointer;">Rapor Ciktisi Al</button>';
    icerik += '</div></div>';
    Metro.dialog.create({
        title: 'Insaat Durum Raporu', content: icerik, closeButton: true, width: 500,
        onDialogCreate: function (dlg) {
            setTimeout(function () {
                zSayac += 100;
                $(dlg).css('z-index', zSayac);
                $('.dialog-overlay').last().css('z-index', zSayac - 1);
            }, 10);
        }
    });
}

function raporuYazdir() {
    var icerik = $('.dialog .dialog-content').html();
    var pencere = window.open('', '_blank');
    pencere.document.write('<html><head><title>Insaat Raporu</title>');
    pencere.document.write('<style>body{font-family:Arial,sans-serif;padding:20px;} table{width:100%;border-collapse:collapse;} td{padding:8px;border:1px solid #ddd;} tr:nth-child(even){background:#f9f9f9;}</style>');
    pencere.document.write('</head><body>');
    pencere.document.write(icerik);
    pencere.document.write('</body></html>');
    pencere.document.close();
    pencere.print();
}

function dosyaSil(dosyaAdi, event) {
    if (event) event.stopPropagation();
    gorselKapat();

    Metro.dialog.create({
        title: "Dosya Silme Onayı",
        content: "<div><b>" + dosyaAdi + "</b> adlı dosyayı silmek istediğinize emin misiniz? Bu işlem geri alınamaz.</div>",
        actions: [
            {
                caption: "Evet, Sil",
                cls: "js-dialog-close alert",
                onclick: function () {
                    $.post('/Medya/DosyaSil', { dosyaAdi: dosyaAdi })
                        .done(function () {
                            Metro.toast.create("Dosya başarıyla silindi!", null, 2000, "bg-green fg-white");


                            $.get(window.location.href, function (html) {
                                var geciciKutu = $('<div>').html(html);
                                var yeniBelgeListe = geciciKutu.find('#belge-icerik-sablonu .galeri-alani').html();
                                if (yeniBelgeListe) {
                                    $('.win-belge .galeri-alani').html(yeniBelgeListe);
                                    $('#belge-icerik-sablonu .galeri-alani').html(yeniBelgeListe);
                                }
                                var yeniMedyaListe = geciciKutu.find('#medya-icerik-sablonu .galeri-alani').html();
                                if (yeniMedyaListe) {
                                    $('.win-medya .galeri-alani').html(yeniMedyaListe);
                                    $('#medya-icerik-sablonu .galeri-alani').html(yeniMedyaListe);
                                }
                            });
                        })
                        .fail(function (xhr) {
                            Metro.toast.create("Hata: " + (xhr.responseText || ""), null, 2000, "bg-red fg-white");
                        });
                }
            },
            { caption: "İptal", cls: "js-dialog-close" }
        ],
        onDialogCreate: function (dlg) {
            setTimeout(function () {
                zSayac += 100;
                $(dlg).css('z-index', zSayac);
                $('.dialog-overlay').last().css('z-index', zSayac - 1);
            }, 10);
        }
    });
}
function paneliSabitle() { panelSabitMi = !panelSabitMi; var pinBtn = document.getElementById('pinButton'); if (panelSabitMi) { pinBtn.classList.add('bg-cyan'); document.body.classList.add('menuler-sabit'); } else { pinBtn.classList.remove('bg-cyan'); document.body.classList.remove('menuler-sabit'); } }
function toggleKendiPanelimiz() { $('#sol-profil-menusu').toggleClass('open'); $('#arka-plan-golgesi').toggleClass('open'); }
function sifreDegistirAc() { $('#sol-profil-menusu').removeClass('open'); $('#arka-plan-golgesi').removeClass('open'); sifreDegistirPenceresiAc(); }
function sifreDegistirPenceresiAc() {
    var icerik = '<form id="sifreDegistirForm" action="/Hesap/SifreGuncelle" method="post" class="p-4"><div class="form-group"><label>Eski Şifre</label><input type="password" name="EskiSifre" data-role="input" required></div><div class="form-group mt-3"><label>Yeni Şifre</label><input type="password" name="YeniSifre" data-role="input" required></div><div class="form-group mt-3"><label>Yeni Şifre (Tekrar)</label><input type="password" name="YeniSifreTekrar" data-role="input" required></div><div class="form-group mt-4 text-right"><button class="button primary">Değiştir</button></div></form>';
    Metro.dialog.create({ title: 'Şifre Değiştir', content: icerik, closeButton: true, width: 400 });
}



$(document).on('submit', '.dosya-yukle-form', function (e) {
    e.preventDefault();
    var form = $(this);
    var fileInput = form.find('input[type="file"]')[0];

    if (!fileInput || !fileInput.files || fileInput.files.length === 0) {
        Metro.toast.create("Lütfen yüklemek için bir dosya seçin!", null, 2000, "bg-red fg-white");
        return false;
    }


    var formData = new FormData(form[0]);

    var btn = form.find('button[type="submit"]');
    var originalText = btn.html();
    btn.html('<span class="mif-spinner4 ani-spin"></span> Yükleniyor...').prop('disabled', true);

    $.ajax({
        url: form.attr('action'),
        type: 'POST',
        data: formData,
        processData: false,
        contentType: false,
        success: function () {
            Metro.toast.create("Dosya başarıyla yüklendi!", null, 2000, "bg-green fg-white");
            btn.html(originalText).prop('disabled', false);
            form[0].reset();


            $.get(window.location.href, function (html) {
                var geciciKutu = $('<div>').html(html);

                var yeniBelgeListe = geciciKutu.find('#belge-icerik-sablonu .galeri-alani').html();
                if (yeniBelgeListe) {
                    $('.win-belge .galeri-alani').html(yeniBelgeListe);
                    $('#belge-icerik-sablonu .galeri-alani').html(yeniBelgeListe);
                }

                var yeniMedyaListe = geciciKutu.find('#medya-icerik-sablonu .galeri-alani').html();
                if (yeniMedyaListe) {
                    $('.win-medya .galeri-alani').html(yeniMedyaListe);
                    $('#medya-icerik-sablonu .galeri-alani').html(yeniMedyaListe);
                }
            });
        },
        error: function () {
            Metro.toast.create("Sunucuya bağlanırken hata oluştu!", null, 2000, "bg-red fg-white");
            btn.html(originalText).prop('disabled', false);
        }
    });
});