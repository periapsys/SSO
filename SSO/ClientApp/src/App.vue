<template>
    <Navbar v-if="showNav" />
    <div v-bind:class="showNav ? 'app-wrapper' : 'row g-0 app-auth-wrapper'">
        <router-view />
        <Footer />
    </div>
    <Loader v-show="loading" />
</template>

<script>
import Navbar from "@/components/Navbar.vue";
import Loader from "@/components/Loader.vue";
import Footer from "@/components/Footer.vue";
import { emitter } from '@/services/emitter.service';

export default {
    components: {
        Navbar, Loader, Footer
    },
    watch: {
        '$route'(to, from) {
            // Check if the current route is in the allowedNavs array
            this.showNav = !this.allowedNavs.includes(to.path);
            document.body.style.backgroundColor = this.showNav ? '#F8F8FF' : '';
        },
    },
    data() {
        return { loading: false, showNav: true, allowedNavs: ['/', '/init', '/login', '/changepassword', '/forgotpassword', '/404'] };
    },
    mounted() {
        if (window.top !== window.self) {
            console.warn('This application cannot be displayed in an iframe.');
            // Optionally, redirect or show a message
            window.top.location = window.location.href;
        }
    },
    created() {
        emitter.on("showLoader", (e) => {
            this.loading = e;
        });
    }
}
</script>

<style>
:root {
    --primary-color: #F80;
    --highlight-color: #fff3e5;
}

.btn-danger,
.btn-danger:hover {
    color: white;
    background-color: #ff534d;
}

.app-btn-outline-primary,
.app-btn-outline-primary:hover {
    background-color: transparent !important;
    color: var(--primary-color);
    border: 1px solid var(--primary-color);
}

.app-btn-outline-secondary,
.app-btn-outline-secondary:hover {
    background-color: transparent !important;
    color: silver;
    border: 1px solid #e7e9ed;
}

.app-btn-outline-danger,
.app-btn-outline-danger:hover {
    background-color: transparent !important;
    color: #ff534d;
    border: 1px solid #ff534d
}

.app-btn-sm {
    padding: 0.2rem 0.4rem;
    /* Adjust padding to make it smaller */
    font-size: 0.75rem;
    /* Adjust font size to make text smaller */
    line-height: 1.3;
    /* Set line height for better text alignment */
    border-radius: 0.15rem;
    /* Add border-radius for rounded corners */
}

a:hover {
    color: var(--primary-color);
}

.sortable {
    cursor: pointer;
}

button.nav-link.active {
    background-color: var(--primary-color) !important;
    color: white;
}

button.nav-link,
button.nav-link:hover {
    color: var(--primary-color);
}

@media (max-width: 767px) {
    .table-responsive .dropdown-menu {
        transform: translate3d(0px, 3px, 0px) !important;
        position: relative !important;
    }
}

@media (min-width: 768px) {
    .table-responsive {
        overflow: inherit;
    }
}

::placeholder {
    /* Chrome, Firefox, Opera, Safari 10.1+ */
    color: silver !important;
    opacity: 1;
    /* Firefox */
}

.autocomplete {
    background: white;
    z-index: 1000;
    font: 14px/22px "-apple-system", BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
    overflow: auto;
    box-sizing: border-box;
    border: 1px solid rgba(50, 50, 50, 0.6);
}

.autocomplete * {
    font: inherit;
}

.autocomplete>div {
    padding: 15px;
}

.autocomplete .group {
    background: #eee;
}

.autocomplete>div:hover:not(.group),
.autocomplete>div.selected {
    background: var(--primary-color);
    opacity: 0.75;
    color: white !important;
    cursor: pointer;
}
</style>