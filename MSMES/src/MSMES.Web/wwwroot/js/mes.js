/**
 * MSMES — 공통 JavaScript
 * Bootstrap 5.3 + Chart.js 기반
 */

/* ── 사이드바 토글 (모바일) ─────────────────────────── */
(function initSidebarToggle() {
    const sidebar  = document.getElementById('msSidebar');
    const toggleBtn = document.getElementById('msSidebarToggle');
    const overlay  = document.getElementById('msOverlay');

    if (!sidebar || !toggleBtn) return;

    function openSidebar() {
        sidebar.classList.add('ms-sidebar--open');
        if (overlay) overlay.style.display = 'block';
        document.body.style.overflow = 'hidden';
    }

    function closeSidebar() {
        sidebar.classList.remove('ms-sidebar--open');
        if (overlay) overlay.style.display = 'none';
        document.body.style.overflow = '';
    }

    toggleBtn.addEventListener('click', function () {
        if (sidebar.classList.contains('ms-sidebar--open')) {
            closeSidebar();
        } else {
            openSidebar();
        }
    });

    if (overlay) {
        overlay.addEventListener('click', closeSidebar);
    }

    // 사이드바 내 nav-link 클릭 시 모바일에서 닫기
    sidebar.querySelectorAll('.ms-sidebar__link').forEach(function(link) {
        link.addEventListener('click', function() {
            if (window.innerWidth < 992) {
                closeSidebar();
            }
        });
    });

    // 화면 크기 변경 시 모바일 상태 초기화
    window.addEventListener('resize', function () {
        if (window.innerWidth >= 992) {
            closeSidebar();
        }
    });
})();

/* ── 활성 메뉴 하이라이트 ───────────────────────────── */
(function highlightActiveMenu() {
    const currentPath = window.location.pathname.toLowerCase();
    const links = document.querySelectorAll('.ms-sidebar__link');
    let bestMatch = null;
    let bestLength = 0;

    links.forEach(function (link) {
        const href = (link.getAttribute('href') || '').toLowerCase();
        if (href === '/' && currentPath === '/') {
            bestMatch = link;
            bestLength = 1;
        } else if (href !== '/' && currentPath.startsWith(href) && href.length > bestLength) {
            bestMatch = link;
            bestLength = href.length;
        }
    });

    if (bestMatch) {
        bestMatch.classList.add('active');
    }
})();

/* ── Toast 알림 ─────────────────────────────────────── */
function showToast(message, type) {
    type = type || 'success';
    var container = document.getElementById('ms-toast-container');
    if (!container) {
        container = document.createElement('div');
        container.id = 'ms-toast-container';
        document.body.appendChild(container);
    }

    var iconMap = {
        success: 'bi-check-circle-fill',
        danger:  'bi-x-circle-fill',
        warning: 'bi-exclamation-triangle-fill',
        info:    'bi-info-circle-fill'
    };

    var toast = document.createElement('div');
    toast.className = 'ms-toast toast-' + type;
    toast.innerHTML =
        '<i class="bi ' + (iconMap[type] || 'bi-info-circle-fill') + '"></i>' +
        '<span>' + message + '</span>';

    container.appendChild(toast);

    setTimeout(function () {
        toast.style.animation = 'ms-toast-in 0.2s ease reverse';
        toast.addEventListener('animationend', function () {
            toast.remove();
        });
    }, 4000);
}

/* ── 삭제 확인 모달 ─────────────────────────────────── */
function confirmDelete(message, callback) {
    message = message || '정말 삭제하시겠습니까?';
    if (confirm(message)) {
        if (typeof callback === 'function') callback();
    }
}

/* ── Fetch 래퍼 (CSRF 토큰 자동 포함) ───────────────── */
function msFetch(url, options) {
    options = options || {};
    var token = document.querySelector('input[name="__RequestVerificationToken"]');
    var headers = Object.assign({
        'Content-Type': 'application/json',
        'X-Requested-With': 'XMLHttpRequest'
    }, options.headers || {});

    if (token) {
        headers['RequestVerificationToken'] = token.value;
    }

    return fetch(url, Object.assign({}, options, { headers: headers }));
}

/* ── 대시보드 차트 초기화 ────────────────────────────── */
var _productionChart = null;
var _equipmentChart  = null;

function initDashboardCharts(data) {
    data = data || {};

    // 생산실적 라인차트
    var prodCtx = document.getElementById('chartProduction');
    if (prodCtx) {
        if (_productionChart) _productionChart.destroy();

        var labels  = (data.productionLabels  || ['월','화','수','목','금','토','일']);
        var prodData   = (data.productionValues  || [0,0,0,0,0,0,0]);
        var defectData = (data.defectValues      || [0,0,0,0,0,0,0]);

        _productionChart = new Chart(prodCtx, {
            type: 'line',
            data: {
                labels: labels,
                datasets: [
                    {
                        label: '생산수량',
                        data: prodData,
                        borderColor: '#1a73e8',
                        backgroundColor: 'rgba(26,115,232,0.08)',
                        borderWidth: 2,
                        pointRadius: 4,
                        pointHoverRadius: 6,
                        fill: true,
                        tension: 0.35
                    },
                    {
                        label: '불량수량',
                        data: defectData,
                        borderColor: '#dc3545',
                        backgroundColor: 'rgba(220,53,69,0.06)',
                        borderWidth: 2,
                        pointRadius: 4,
                        pointHoverRadius: 6,
                        fill: true,
                        tension: 0.35
                    }
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: { mode: 'index', intersect: false },
                plugins: {
                    legend: {
                        position: 'top',
                        labels: { font: { size: 12, family: 'Noto Sans KR' } }
                    },
                    tooltip: {
                        callbacks: {
                            label: function(ctx) {
                                return ctx.dataset.label + ': ' + ctx.parsed.y.toLocaleString('ko-KR') + '개';
                            }
                        }
                    }
                },
                scales: {
                    x: {
                        grid: { color: 'rgba(0,0,0,0.04)' },
                        ticks: { font: { size: 12 } }
                    },
                    y: {
                        beginAtZero: true,
                        grid: { color: 'rgba(0,0,0,0.04)' },
                        ticks: {
                            font: { size: 12 },
                            callback: function(v) { return v.toLocaleString('ko-KR'); }
                        }
                    }
                }
            }
        });
    }

    // 설비 현황 도넛차트
    var eqCtx = document.getElementById('chartEquipment');
    if (eqCtx) {
        if (_equipmentChart) _equipmentChart.destroy();

        var eqData   = data.equipmentValues  || [0, 0, 0, 0];
        var totalEq  = eqData.reduce(function(a, b) { return a + b; }, 0);
        var runningPct = totalEq > 0 ? Math.round((eqData[0] / totalEq) * 100) : 0;

        _equipmentChart = new Chart(eqCtx, {
            type: 'doughnut',
            data: {
                labels: ['가동', '정지', '점검', '고장'],
                datasets: [{
                    data: eqData,
                    backgroundColor: ['#28a745', '#6c757d', '#ffc107', '#dc3545'],
                    borderWidth: 2,
                    borderColor: '#fff'
                }]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                cutout: '68%',
                plugins: {
                    legend: {
                        position: 'bottom',
                        labels: { font: { size: 12, family: 'Noto Sans KR' }, padding: 12 }
                    },
                    tooltip: {
                        callbacks: {
                            label: function(ctx) {
                                var total = ctx.dataset.data.reduce(function(a, b) { return a + b; }, 0);
                                var pct   = total > 0 ? Math.round((ctx.parsed / total) * 100) : 0;
                                return ctx.label + ': ' + ctx.parsed + '대 (' + pct + '%)';
                            }
                        }
                    }
                }
            },
            plugins: [{
                id: 'centerText',
                afterDraw: function(chart) {
                    var ctx2 = chart.ctx;
                    var cx   = chart.chartArea.left + (chart.chartArea.right  - chart.chartArea.left) / 2;
                    var cy   = chart.chartArea.top  + (chart.chartArea.bottom - chart.chartArea.top)  / 2;
                    ctx2.save();
                    ctx2.textAlign    = 'center';
                    ctx2.textBaseline = 'middle';
                    ctx2.fillStyle    = '#1a73e8';
                    ctx2.font         = 'bold 20px Noto Sans KR, sans-serif';
                    ctx2.fillText(runningPct + '%', cx, cy - 8);
                    ctx2.fillStyle = '#6c757d';
                    ctx2.font      = '11px Noto Sans KR, sans-serif';
                    ctx2.fillText('가동률', cx, cy + 12);
                    ctx2.restore();
                }
            }]
        });
    }
}

/* ── 대시보드 KPI AJAX 로드 ──────────────────────────── */
function loadDashboardData() {
    // KPI 스켈레톤 표시
    document.querySelectorAll('.kpi-card__value[data-kpi]').forEach(function(el) {
        el.classList.add('skeleton');
        el.textContent = '';
    });

    msFetch('/api/dashboard')
        .then(function(res) {
            if (!res.ok) throw new Error('API 오류: ' + res.status);
            return res.json();
        })
        .then(function(data) {
            // KPI 카드 채우기
            document.querySelectorAll('[data-kpi]').forEach(function(el) {
                var key = el.getAttribute('data-kpi');
                el.classList.remove('skeleton');
                if (data[key] !== undefined) {
                    el.textContent = data[key];
                }
            });

            // 차트 초기화
            initDashboardCharts(data);
        })
        .catch(function(err) {
            console.warn('대시보드 API 호출 실패, 샘플 데이터 사용:', err.message);

            // 샘플 데이터로 폴백
            var today = new Date();
            var labels = [];
            for (var i = 6; i >= 0; i--) {
                var d = new Date(today);
                d.setDate(d.getDate() - i);
                labels.push((d.getMonth()+1) + '/' + d.getDate());
            }

            initDashboardCharts({
                productionLabels: labels,
                productionValues: [820, 930, 1010, 880, 1048, 970, 1100],
                defectValues:     [12,  18,  8,   22,  15,   9,   11],
                equipmentValues:  [8, 2, 1, 1]
            });

            // KPI 샘플값
            var samples = {
                todayProduced:   '1,048',
                todayTarget:     '1,000',
                openOrderCount:  '23',
                openOrderAmount: '₩ 4.8억',
                lowStockCount:   '5',
                equipmentRate:   '72%',
                defectRate:      '1.43%',
                activeWorkOrders:'18'
            };

            document.querySelectorAll('[data-kpi]').forEach(function(el) {
                var key = el.getAttribute('data-kpi');
                el.classList.remove('skeleton');
                if (samples[key] !== undefined) {
                    el.textContent = samples[key];
                }
            });
        });
}

/* ── DataTable 공통 초기화 ───────────────────────────── */
(function initDataTables() {
    if (typeof $.fn === 'undefined' || typeof $.fn.DataTable === 'undefined') return;

    document.querySelectorAll('table.mes-table[data-datatable]').forEach(function(table) {
        $(table).DataTable({
            language: {
                url: '/lib/datatables/i18n/ko.json',
                emptyTable:     '데이터가 없습니다',
                zeroRecords:    '검색 결과가 없습니다',
                info:           '_START_ - _END_ / 전체 _TOTAL_건',
                infoEmpty:      '0건',
                infoFiltered:   '(전체 _MAX_건 중 검색)',
                search:         '검색:',
                paginate: {
                    first:    '처음',
                    last:     '마지막',
                    next:     '다음',
                    previous: '이전'
                },
                lengthMenu: '_MENU_ 건씩 보기'
            },
            pageLength: 10,
            lengthMenu: [10, 25, 50],
            order: [],
            responsive: true
        });
    });
})();

/* ── 폼 Bootstrap validation ─────────────────────────── */
(function initFormValidation() {
    document.querySelectorAll('form.needs-validation').forEach(function(form) {
        form.addEventListener('submit', function(event) {
            if (!form.checkValidity()) {
                event.preventDefault();
                event.stopPropagation();
                // 첫 번째 invalid 필드로 스크롤
                var firstInvalid = form.querySelector(':invalid');
                if (firstInvalid) firstInvalid.focus();
            }
            form.classList.add('was-validated');
        }, false);
    });
})();

/* ── 모달 Esc 닫기 / Enter 저장 ─────────────────────── */
document.addEventListener('keydown', function(e) {
    if (e.key === 'Escape') {
        var openModal = document.querySelector('.modal.show');
        if (openModal) {
            var bsModal = bootstrap.Modal.getInstance(openModal);
            if (bsModal) bsModal.hide();
        }
    }
    if (e.key === 'Enter' && e.target.tagName !== 'TEXTAREA' && e.target.tagName !== 'BUTTON') {
        var openModal = document.querySelector('.modal.show');
        if (openModal) {
            var saveBtn = openModal.querySelector('[data-save]');
            if (saveBtn) saveBtn.click();
        }
    }
});

/* ── 대시보드 페이지 자동 실행 ───────────────────────── */
document.addEventListener('DOMContentLoaded', function() {
    if (document.getElementById('chartProduction') || document.getElementById('chartEquipment')) {
        loadDashboardData();
    }
});
