document.addEventListener('DOMContentLoaded', function () {
    var btnGrafik = document.getElementById('btnGrafik');
    if (!btnGrafik) return;

    btnGrafik.addEventListener('click', function () {
        fetch('/Harita/InsaatlariGetir')
            .then(function (res) { return res.json(); })
            .then(function (data) {
                if (!data.success) return;

                var insaatlar = data.data;
                var devamEden = insaatlar.filter(function (i) { return i.durumId === 1; }).length;
                var durduruldu = insaatlar.filter(function (i) { return i.durumId === 0; }).length;
                var tamamlandi = insaatlar.filter(function (i) { return i.durumId === 2; }).length;

                var aylar = {};
                insaatlar.forEach(function (i) {
                    if (i.baslamaTarihi) {
                        var t = new Date(i.baslamaTarihi);
                        var ay = t.getFullYear() + '-' + String(t.getMonth() + 1).padStart(2, '0');
                        aylar[ay] = (aylar[ay] || 0) + 1;
                    }
                });
                var sortedAylar = Object.keys(aylar).sort();

                Metro.dialog.open('#grafikDialog');

                setTimeout(function () {
                    if (window.durumChart) window.durumChart.destroy();
                    if (window.aylikChart) window.aylikChart.destroy();

                    window.durumChart = new Chart(document.getElementById('grafikDurum').getContext('2d'), {
                        type: 'doughnut',
                        data: {
                            labels: ['Devam Ediyor', 'Durduruldu', 'Tamamlandi'],
                            datasets: [{ data: [devamEden, durduruldu, tamamlandi], backgroundColor: ['#f39c12', '#e74c3c', '#27ae60'] }]
                        }
                    });

                    
                    window.aylikChart = new Chart(document.getElementById('grafikAylik').getContext('2d'), {
                        type: 'bar',
                        data: {
                            labels: sortedAylar,
                            datasets: [{ label: 'Insaat Sayisi', data: sortedAylar.map(function (a) { return aylar[a]; }), backgroundColor: '#3498db' }]
                        }
                    });

                    // CİNSİYET DROPDOWN'U DOLDUR
                    var secici = document.getElementById('grafikInsaatSecimi');
                    if (secici) {
                        secici.innerHTML = '<option value="">İnşaat seçiniz...</option>';
                        insaatlar.forEach(function (i) {
                            var opt = document.createElement('option');
                            opt.value = i.id;
                            opt.textContent = i.insaatAdi;
                            secici.appendChild(opt);
                        });

                        // Seçim değiştiğinde cinsiyet grafiğini çiz
                        secici.onchange = function () {
                            var secilenId = parseInt(this.value);
                            if (window.cinsiyetChart) window.cinsiyetChart.destroy();
                            if (!secilenId) return;

                            var secilen = insaatlar.find(function (i) { return i.id === secilenId; });
                            if (!secilen) return;

                            var aPersoneller = secilen.aPersoneller || secilen.APersoneller || [];
                            var bPersoneller = secilen.bPersoneller || secilen.BPersoneller || [];
                            var tumPersoneller = aPersoneller.concat(bPersoneller);

                            var erkek = 0, kadin = 0;
                            tumPersoneller.forEach(function (p) {
                                if (p.cinsiyet === 'Erkek') erkek++;
                                else if (p.cinsiyet === 'Kadın') kadin++;
                            });

                            window.cinsiyetChart = new Chart(document.getElementById('grafikCinsiyet').getContext('2d'), {
                                type: 'doughnut',
                                data: {
                                    labels: ['Erkek', 'Kadin'],
                                    datasets: [{
                                        data: [erkek, kadin],
                                        backgroundColor: ['#3498db', '#e91e63']
                                    }]
                                }
                            });
                        };
                    }
                }, 300);
              
            });
    });
});