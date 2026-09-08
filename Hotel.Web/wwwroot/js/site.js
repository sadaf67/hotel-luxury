/* =============================================
   هتل لوکس پارسیان — JavaScript اصلی
   شامل: تم دارک/لایت، دوزبانه، اسلایدر، منوی موبایل
   ============================================= */

/* =============================================
   ۱) THEME — دارک / لایت
   ============================================= */
(function initTheme() {
    const saved = localStorage.getItem('hotelTheme') || 'dark';
    document.documentElement.setAttribute('data-theme', saved);
    document.documentElement.classList.toggle('light-mode', saved === 'light');
})();

function toggleTheme() {
    const current = document.documentElement.getAttribute('data-theme') || 'dark';
    const next = current === 'dark' ? 'light' : 'dark';
    document.documentElement.setAttribute('data-theme', next);
    localStorage.setItem('hotelTheme', next);
    // به‌روزرسانی آیکون دکمه
    document.querySelectorAll('.theme-toggle i').forEach(ic => {
        ic.className = next === 'dark' ? 'fas fa-sun' : 'fas fa-moon';
    });
}

/* =============================================
   ۲) LANGUAGE — فارسی (fa) / انگلیسی (en)
   ============================================= */
const translations = {
    fa: {
        nav_home:    'خانه',
        nav_rooms:   'اتاق‌ها',
        nav_pool:    'استخر',
        nav_cafe:    'کافی‌شاپ',
        nav_gallery: 'گالری',
        nav_contact: 'تماس',
        nav_reserve: 'رزرو کنید',
        hero_title:  'تجربه‌ای <span>بی‌نظیر</span> از اقامت لوکس',
        hero_sub:    'هتل لوکس پارسیان — جایی که آسایش با شکوه در هم می‌آمیزد',
        hero_btn1:   'رزرو اتاق',
        hero_btn2:   'بیشتر بدانید',
        feat_rooms:  'اتاق‌های لوکس',
        feat_pool:   'استخر سرپوشیده',
        feat_cafe:   'کافی‌شاپ',
        feat_rest:   'رستوران',
        feat_spa:    'سپا و ماساژ',
        feat_park:   'پارکینگ',
        sec_rooms:   'اتاق‌های <span>ما</span>',
        sec_pool:    'استخر <span>لوکس</span>',
        sec_cafe:    'کافی‌شاپ <span>پارسیان</span>',
        per_night:   'هر شب',
        reserve_btn: 'رزرو این اتاق',
        view_all:    'مشاهده همه',
        toman:       'تومان',
    },
    en: {
        nav_home:    'Home',
        nav_rooms:   'Rooms',
        nav_pool:    'Pool',
        nav_cafe:    'Café',
        nav_gallery: 'Gallery',
        nav_contact: 'Contact',
        nav_reserve: 'Book Now',
        hero_title:  'An <span>Extraordinary</span> Luxury Stay',
        hero_sub:    'Parsian Luxury Hotel — where comfort meets grandeur',
        hero_btn1:   'Book a Room',
        hero_btn2:   'Learn More',
        feat_rooms:  'Luxury Rooms',
        feat_pool:   'Indoor Pool',
        feat_cafe:   'Café',
        feat_rest:   'Restaurant',
        feat_spa:    'Spa & Massage',
        feat_park:   'Parking',
        sec_rooms:   'Our <span>Rooms</span>',
        sec_pool:    'Luxury <span>Pool</span>',
        sec_cafe:    'Parsian <span>Café</span>',
        per_night:   'per night',
        reserve_btn: 'Reserve Room',
        view_all:    'View All',
        toman:       'TMN',
    }
};

(function initLang() {
    const saved = localStorage.getItem('hotelLang') || 'fa';
    applyLanguage(saved, false);
})();

function switchLanguage(lang) {
    applyLanguage(lang, true);
    localStorage.setItem('hotelLang', lang);
}

function applyLanguage(lang, animate) {
    const html = document.documentElement;
    html.setAttribute('lang', lang);
    document.body.setAttribute('dir', lang === 'fa' ? 'rtl' : 'ltr');
    document.body.classList.toggle('rtl', lang === 'fa');
    document.body.classList.toggle('ltr', lang === 'en');

    // به‌روزرسانی دکمه‌های زبان
    document.querySelectorAll('.lang-btn').forEach(b => {
        b.classList.toggle('active', b.dataset.lang === lang);
    });

    // ترجمه المان‌های دارای data-i18n
    const t = translations[lang] || translations.fa;
    document.querySelectorAll('[data-i18n]').forEach(el => {
        const key = el.getAttribute('data-i18n');
        if (t[key] !== undefined) {
            if (el.tagName === 'INPUT' || el.tagName === 'TEXTAREA') {
                el.placeholder = t[key];
            } else {
                el.innerHTML = t[key];
            }
        }
    });

    if (animate) {
        document.body.style.opacity = '0';
        setTimeout(() => { document.body.style.opacity = '1'; }, 200);
        document.body.style.transition = 'opacity .2s ease';
    }
}

/* =============================================
   ۳) NAVBAR — اسکرول + موبایل
   ============================================= */
document.addEventListener('DOMContentLoaded', () => {
    /* ---- ACTIVE NAV + BOOKING DATES ---- */
    initActiveNavigation();
    initBookingDates();
    // اسکرول navbar
    const navbar = document.getElementById('navbar');
    if (navbar) {
        window.addEventListener('scroll', () => {
            navbar.classList.toggle('scrolled', window.scrollY > 60);
        }, { passive: true });
    }

    // موبایل toggle
    const toggle  = document.getElementById('navToggle');
    const drawer  = document.getElementById('navDrawer');
    const overlay = document.getElementById('navOverlay');

    if (toggle && drawer) {
        toggle.addEventListener('click', () => {
            const open = drawer.classList.toggle('open');
            toggle.classList.toggle('open', open);
            if (overlay) overlay.classList.toggle('show', open);
            document.body.style.overflow = open ? 'hidden' : '';
        });
    }

    if (overlay) {
        overlay.addEventListener('click', closeDrawer);
    }

    function closeDrawer() {
        if (!drawer) return;
        drawer.classList.remove('open');
        if (toggle) toggle.classList.remove('open');
        if (overlay) overlay.classList.remove('show');
        document.body.style.overflow = '';
    }

    // بستن drawer هنگام کلیک روی لینک
    document.querySelectorAll('.nav-drawer-link').forEach(link => {
        link.addEventListener('click', closeDrawer);
    });

    // تنظیم آیکون تم بر اساس تنظیمات فعلی
    const currentTheme = document.documentElement.getAttribute('data-theme') || 'dark';
    document.querySelectorAll('.theme-toggle i').forEach(ic => {
        ic.className = currentTheme === 'dark' ? 'fas fa-sun' : 'fas fa-moon';
    });

    /* ---- SLIDER ---- */
    initSlider();

    /* ---- CAFE TABS ---- */
    initCafeTabs();

    /* ---- GALLERY LIGHTBOX ---- */
    initLightbox();

    /* ---- COUNTER ANIMATION ---- */
    initCounters();

    /* ---- SCROLL ANIMATIONS ---- */
    initScrollAnim();

    /* ---- TOAST AUTO-REMOVE ---- */
    const toast = document.getElementById('alert-toast');
    if (toast) setTimeout(() => toast.remove(), 4000);
});

function initActiveNavigation() {
    const path = window.location.pathname.toLowerCase().replace(/\/$/, '') || '/';
    document.querySelectorAll('.nav-link, .nav-drawer-link').forEach(link => {
        const href = (link.getAttribute('href') || '').toLowerCase().replace(/\/$/, '') || '/';
        if (href === path || (href !== '/' && path.startsWith(href))) link.classList.add('active');
    });
}

function initBookingDates() {
    const form = document.getElementById('bookingDock');
    if (!form) return;
    const arrival = form.querySelector('[name="checkIn"]');
    const departure = form.querySelector('[name="checkOut"]');
    if (!arrival || !departure) return;
    arrival.addEventListener('change', () => {
        if (!arrival.value) return;
        const next = new Date(arrival.value + 'T00:00:00');
        next.setDate(next.getDate() + 1);
        const minDeparture = next.toISOString().slice(0, 10);
        departure.min = minDeparture;
        if (!departure.value || departure.value <= arrival.value) departure.value = minDeparture;
    });
}

/* =============================================
   ۴) HERO SLIDER
   ============================================= */
function initSlider() {
    const slides = document.querySelectorAll('.slide');
    const dots   = document.querySelectorAll('.dot');
    if (!slides.length) return;

    let current = 0, timer = null;

    function goTo(n) {
        slides[current].classList.remove('active');
        if (dots[current]) dots[current].classList.remove('active');
        current = (n + slides.length) % slides.length;
        slides[current].classList.add('active');
        if (dots[current]) dots[current].classList.add('active');
    }

    function startAuto() {
        clearInterval(timer);
        timer = setInterval(() => goTo(current + 1), 5000);
    }

    // initialize
    slides[0].classList.add('active');
    if (dots[0]) dots[0].classList.add('active');
    startAuto();

    // دکمه‌های قبلی/بعدی
    const prevBtn = document.querySelector('.slider-prev');
    const nextBtn = document.querySelector('.slider-next');
    if (prevBtn) prevBtn.addEventListener('click', () => { goTo(current - 1); startAuto(); });
    if (nextBtn) nextBtn.addEventListener('click', () => { goTo(current + 1); startAuto(); });

    // نقاط dot
    dots.forEach((dot, i) => {
        dot.addEventListener('click', () => { goTo(i); startAuto(); });
    });

    // touch/swipe
    let touchX = 0;
    const hero = document.querySelector('.hero');
    if (hero) {
        hero.addEventListener('touchstart', e => { touchX = e.touches[0].clientX; }, { passive: true });
        hero.addEventListener('touchend', e => {
            const diff = touchX - e.changedTouches[0].clientX;
            if (Math.abs(diff) > 40) { goTo(diff > 0 ? current + 1 : current - 1); startAuto(); }
        }, { passive: true });
    }
}

/* =============================================
   ۵) CAFE TABS
   ============================================= */
function initCafeTabs() {
    const tabs = document.querySelectorAll('.cafe-tab');
    const sections = document.querySelectorAll('.menu-section');

    if (!tabs.length) return;

    tabs.forEach(tab => {
        tab.addEventListener('click', () => {
            const target = tab.dataset.cat;
            tabs.forEach(t => t.classList.remove('active'));
            tab.classList.add('active');

            if (target === 'all') {
                sections.forEach(s => { s.style.display = ''; });
            } else {
                sections.forEach(s => {
                    s.style.display = s.dataset.cat === target ? '' : 'none';
                });
            }
        });
    });

    // باز کردن اولین تب
    if (tabs[0]) tabs[0].click();
}

/* =============================================
   ۶) GALLERY LIGHTBOX
   ============================================= */
function initLightbox() {
    const lb    = document.getElementById('lightbox');
    const lbImg = document.getElementById('lightboxImg');
    if (!lb || !lbImg) return;

    document.querySelectorAll('.gallery-item').forEach(item => {
        item.addEventListener('click', () => {
            const src = item.querySelector('img')?.src;
            if (src) { lbImg.src = src; lb.classList.add('open'); document.body.style.overflow = 'hidden'; }
        });
    });

    function closeLb() { lb.classList.remove('open'); document.body.style.overflow = ''; }
    document.getElementById('lightboxClose')?.addEventListener('click', closeLb);
    lb.addEventListener('click', e => { if (e.target === lb) closeLb(); });
    document.addEventListener('keydown', e => { if (e.key === 'Escape') closeLb(); });
}

/* =============================================
   ۷) COUNTER ANIMATION
   ============================================= */
function initCounters() {
    const counters = document.querySelectorAll('[data-count]');
    if (!counters.length) return;

    const observer = new IntersectionObserver(entries => {
        entries.forEach(entry => {
            if (!entry.isIntersecting) return;
            const el  = entry.target;
            const end = parseInt(el.dataset.count, 10);
            const dur = 2000;
            const step = end / (dur / 16);
            let current = 0;
            const tick = () => {
                current = Math.min(current + step, end);
                el.textContent = Math.floor(current).toLocaleString('fa-IR');
                if (current < end) requestAnimationFrame(tick);
            };
            requestAnimationFrame(tick);
            observer.unobserve(el);
        });
    }, { threshold: 0.5 });

    counters.forEach(c => observer.observe(c));
}

/* =============================================
   ۸) SCROLL ANIMATIONS
   ============================================= */
function initScrollAnim() {
    const els = document.querySelectorAll('.anim-up, .anim-fade');
    if (!els.length) return;

    // استایل اولیه
    els.forEach(el => {
        el.style.opacity = '0';
        if (el.classList.contains('anim-up')) el.style.transform = 'translateY(36px)';
        el.style.transition = 'opacity .6s ease, transform .6s ease';
    });

    const io = new IntersectionObserver(entries => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.style.opacity = '1';
                entry.target.style.transform = 'translateY(0)';
                io.unobserve(entry.target);
            }
        });
    }, { threshold: 0.12 });

    els.forEach(el => io.observe(el));
}

/* =============================================
   ۹) AVAILABILITY CHECK (AJAX)
   ============================================= */
function checkAvailability() {
    const roomId   = document.getElementById('RoomId')?.value;
    const checkIn  = document.getElementById('CheckIn')?.value;
    const checkOut = document.getElementById('CheckOut')?.value;
    const resultEl = document.getElementById('availability-result');

    if (!roomId || !checkIn || !checkOut || !resultEl) return;

    fetch('/Reservation/CheckAvailability', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': document.querySelector('[name="__RequestVerificationToken"]')?.value || ''
        },
        body: `roomId=${roomId}&checkIn=${checkIn}&checkOut=${checkOut}&__RequestVerificationToken=${encodeURIComponent(document.querySelector('[name="__RequestVerificationToken"]')?.value || '')}`
    })
    .then(r => r.json())
    .then(data => {
        resultEl.innerHTML = '';

        const box = document.createElement('div');
        box.className = `availability-box ${data.available ? 'avail-ok' : 'avail-no'}`;
        box.innerHTML = data.available
            ? `<i class="fas fa-check-circle"></i> اتاق آزاد است — ${data.nights} شب — جمع: <strong>${data.total} تومان</strong>`
            : `<i class="fas fa-times-circle"></i> ${data.message}`;
        resultEl.appendChild(box);

        // به‌روز‌رسانی جمع قیمت
        if (data.available) updatePriceSummary(data.total, data.nights);
    })
    .catch(console.error);
}

/* =============================================
   ۱۰) DISCOUNT VALIDATION (AJAX)
   ============================================= */
function validateDiscount() {
    const code    = document.getElementById('DiscountCode')?.value?.trim();
    const totalEl = document.getElementById('total-price');
    const msgEl   = document.getElementById('discount-msg');
    if (!code || !totalEl || !msgEl) return;

    const total = totalEl.dataset.raw || '0';

    fetch('/Reservation/ValidateDiscount', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/x-www-form-urlencoded',
            'RequestVerificationToken': document.querySelector('[name="__RequestVerificationToken"]')?.value || ''
        },
        body: `code=${encodeURIComponent(code)}&total=${total}&__RequestVerificationToken=${encodeURIComponent(document.querySelector('[name="__RequestVerificationToken"]')?.value || '')}`
    })
    .then(r => r.json())
    .then(data => {
        msgEl.innerHTML = data.valid
            ? `<span style="color:#2ecc71"><i class="fas fa-tag"></i> تخفیف ${data.amount} تومان اعمال شد</span>`
            : `<span style="color:#e74c3c"><i class="fas fa-times"></i> ${data.message}</span>`;

        if (data.valid) {
            const discRow = document.getElementById('discount-row');
            if (discRow) { discRow.style.display = ''; discRow.querySelector('.dval').textContent = data.amount + ' تومان'; }
            const finalEl = document.getElementById('final-price');
            if (finalEl) {
                const rawTotal = parseInt(total.replace(/,/g, ''), 10);
                const disc     = parseInt(data.amount.replace(/,/g, ''), 10);
                finalEl.textContent = (rawTotal - disc).toLocaleString('fa-IR') + ' تومان';
            }
        }
    })
    .catch(console.error);
}

function updatePriceSummary(totalStr, nights) {
    const nightsEl = document.getElementById('summary-nights');
    const totalEl  = document.getElementById('summary-total');
    const finalEl  = document.getElementById('final-price');
    if (nightsEl) nightsEl.textContent = nights + ' شب';
    if (totalEl)  { totalEl.textContent = totalStr + ' تومان'; totalEl.dataset.raw = totalStr; }
    if (finalEl)  finalEl.textContent = totalStr + ' تومان';
    const summaryBox = document.getElementById('price-summary-box');
    if (summaryBox) summaryBox.style.display = '';
}

/* =============================================
   ۱۱) ADMIN — تغییر وضعیت (AJAX)
   ============================================= */
function updateStatus(id, status, token) {
    if (!confirm('آیا مطمئن هستید؟')) return;
    fetch('/Admin/Reservations/UpdateStatus', {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `id=${id}&status=${status}&__RequestVerificationToken=${token}`
    })
    .then(r => r.json())
    .then(d => { if (d.success) location.reload(); else alert('خطا در به‌روزرسانی'); });
}

function deleteItem(url, token) {
    if (!confirm('آیا از حذف مطمئن هستید؟')) return;
    fetch(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
        body: `__RequestVerificationToken=${token}`
    })
    .then(r => r.json())
    .then(d => { if (d.success) location.reload(); else alert('خطا در حذف'); });
}

/* =============================================
   ۱۲) ADMIN SIDEBAR (موبایل)
   ============================================= */
function toggleAdminSidebar() {
    const sidebar = document.getElementById('adminSidebar');
    if (sidebar) sidebar.classList.toggle('open');
}
