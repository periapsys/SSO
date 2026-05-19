<template>
    <header class="app-header fixed-top">
        <div class="app-header-inner">
            <div class="container-fluid py-2">
                <div class="app-header-content">
                    <div class="row justify-content-between align-items-center">
                        <div class="col-auto">
                            <a id="sidepanel-toggler" class="sidepanel-toggler d-inline-block d-xl-none" href="#"
                                @click="onMenuClick()">
                                <i class="fas fa-bars"></i>
                            </a>
                        </div>

                        <div class="app-utilities col-auto">
                            <div class="app-utility-item">
                                <span class="text-muted">SSO</span>
                            </div>

                            <div class="app-utility-item app-user-dropdown dropdown">
                                <a class="dropdown-toggle" id="user-dropdown-toggle" data-bs-toggle="dropdown" href="#"
                                    role="button" aria-expanded="false">
                                    <i class="far fa-user-circle fs-4"></i>
                                </a>
                                <ul class="dropdown-menu" aria-labelledby="user-dropdown-toggle">
                                    <li>
                                        <router-link to="/changepassword" class="dropdown-item" href="#">
                                            Change password
                                        </router-link>
                                    </li>
                                    <li>
                                        <button class="dropdown-item" href="#" @click="on2faClick">
                                            2FA
                                        </button>
                                    </li>
                                    <li>
                                        <button class="dropdown-item" href="#" @click="signout">
                                            Log Out
                                        </button>
                                    </li>
                                </ul>
                            </div>
                            <!--//app-user-dropdown-->
                        </div>
                        <!--//app-utilities-->
                    </div>
                    <!--//row-->
                </div>
                <!--//app-header-content-->
            </div>
            <!--//container-fluid-->
        </div>
        <!--//app-header-inner-->
        <div id="app-sidepanel" class="app-sidepanel">
            <div id="sidepanel-drop" class="sidepanel-drop"></div>
            <div class="sidepanel-inner d-flex flex-column">
                <a href="#" id="sidepanel-close" class="sidepanel-close d-xl-none" @click="onMenuClose()">&times;</a>
                <div class="app-branding">
                    <router-link to="/main" class="app-logo">
                        <img class="logo-icon me-2" :src="require('@/assets/logo.png')" alt="logo" /><span
                            class="logo-text">SSO</span>
                    </router-link>
                </div>
                <!--//app-branding-->

                <nav id="app-nav-main" class="app-nav app-nav-main flex-grow-1">
                    <ul class="app-menu list-unstyled accordion" id="menu-accordion">
                        <li class="nav-item">
                            <!--//Bootstrap Icons: https://icons.getbootstrap.com/ -->

                            <router-link to="/main" class="nav-link">
                                <span class="nav-icon">
                                    <i class="fas fa-home"></i>
                                </span>
                                <span class="nav-link-text">Main</span>
                            </router-link>
                            <!--//nav-link-->
                        </li>
                        <li class="nav-item has-submenu">
                            <!--//Bootstrap Icons: https://icons.getbootstrap.com/ -->
                            <a class="nav-link submenu-toggle" href="/applications" data-bs-toggle="collapse"
                                data-bs-target="#submenu-1" aria-expanded="false" aria-controls="submenu-1">
                                <span class="nav-icon">
                                    <!--//Bootstrap Icons: https://icons.getbootstrap.com/ -->
                                    <i class="fas fa-cubes"></i>
                                </span>
                                <span class="nav-link-text">Applications</span>
                                <span class="submenu-arrow">
                                    <svg width="1em" height="1em" viewBox="0 0 16 16" class="bi bi-chevron-down"
                                        fill="currentColor" xmlns="http://www.w3.org/2000/svg">
                                        <path fill-rule="evenodd"
                                            d="M1.646 4.646a.5.5 0 0 1 .708 0L8 10.293l5.646-5.647a.5.5 0 0 1 .708.708l-6 6a.5.5 0 0 1-.708 0l-6-6a.5.5 0 0 1 0-.708z" />
                                    </svg>
                                </span>
                                <!--//submenu-arrow-->
                            </a>
                            <!--//nav-link-->
                            <div id="submenu-1" class="collapse submenu submenu-1" data-bs-parent="#menu-accordion">
                                <ul class="submenu-list list-unstyled">
                                    <li class="submenu-item">
                                        <router-link to="/applications/new" class="submenu-link">Create
                                            New</router-link>
                                    </li>
                                    <li class="submenu-item">
                                        <router-link to="/applications" class="submenu-link">Manage</router-link>
                                    </li>
                                </ul>
                            </div>
                        </li>
                        <li class="nav-item has-submenu">
                            <!--//Bootstrap Icons: https://icons.getbootstrap.com/ -->
                            <a class="nav-link submenu-toggle" href="/users" data-bs-toggle="collapse"
                                data-bs-target="#submenu-2" aria-expanded="false" aria-controls="submenu-2">
                                <span class="nav-icon">
                                    <!--//Bootstrap Icons: https://icons.getbootstrap.com/ -->
                                    <i class="fas fa-users"></i>
                                </span>
                                <span class="nav-link-text">Users</span>
                                <span class="submenu-arrow">
                                    <svg width="1em" height="1em" viewBox="0 0 16 16" class="bi bi-chevron-down"
                                        fill="currentColor" xmlns="http://www.w3.org/2000/svg">
                                        <path fill-rule="evenodd"
                                            d="M1.646 4.646a.5.5 0 0 1 .708 0L8 10.293l5.646-5.647a.5.5 0 0 1 .708.708l-6 6a.5.5 0 0 1-.708 0l-6-6a.5.5 0 0 1 0-.708z" />
                                    </svg>
                                </span>
                                <!--//submenu-arrow-->
                            </a>
                            <!--//nav-link-->
                            <div id="submenu-2" class="collapse submenu submenu-1" data-bs-parent="#menu-accordion">
                                <ul class="submenu-list list-unstyled">
                                    <li class="submenu-item">
                                        <router-link to="/users/new" class="submenu-link">Create New</router-link>
                                    </li>
                                    <li class="submenu-item">
                                        <router-link to="/users" class="submenu-link">Manage</router-link>
                                    </li>
                                </ul>
                            </div>
                        </li>
                        <li class="nav-item has-submenu">
                            <!--//Bootstrap Icons: https://icons.getbootstrap.com/ -->
                            <a class="nav-link submenu-toggle" href="/groups" data-bs-toggle="collapse"
                                data-bs-target="#submenu-3" aria-expanded="false" aria-controls="submenu-3">
                                <span class="nav-icon">
                                    <!--//Bootstrap Icons: https://icons.getbootstrap.com/ -->
                                    <i class="fas fa-users-rectangle"></i>
                                </span>
                                <span class="nav-link-text">Groups</span>
                                <span class="submenu-arrow">
                                    <svg width="1em" height="1em" viewBox="0 0 16 16" class="bi bi-chevron-down"
                                        fill="currentColor" xmlns="http://www.w3.org/2000/svg">
                                        <path fill-rule="evenodd"
                                            d="M1.646 4.646a.5.5 0 0 1 .708 0L8 10.293l5.646-5.647a.5.5 0 0 1 .708.708l-6 6a.5.5 0 0 1-.708 0l-6-6a.5.5 0 0 1 0-.708z" />
                                    </svg>
                                </span>
                                <!--//submenu-arrow-->
                            </a>
                            <!--//nav-link-->
                            <div id="submenu-3" class="collapse submenu submenu-1" data-bs-parent="#menu-accordion">
                                <ul class="submenu-list list-unstyled">
                                    <li class="submenu-item">
                                        <router-link to="/groups/new" class="submenu-link">Create New</router-link>
                                    </li>
                                    <li class="submenu-item">
                                        <router-link to="/groups" class="submenu-link">Manage</router-link>
                                    </li>
                                </ul>
                            </div>
                        </li>
                    </ul>
                    <!--//app-menu-->
                </nav>
                <!--//app-nav-->
                <div class="app-sidepanel-footer">
                    <nav class="app-nav app-nav-footer">
                        <ul class="app-menu footer-menu list-unstyled">
                            <!--//nav-item-->
                            <li class="nav-item">
                                <!--//Bootstrap Icons: https://icons.getbootstrap.com/ -->
                                <router-link to="/settings" class="nav-link">
                                    <span class="nav-icon">
                                        <i class="bi bi-gear"></i>
                                    </span>
                                    <span class="nav-link-text">Settings</span> </router-link><!--//nav-link-->
                            </li>
                            <!--//nav-item-->
                            <li class="nav-item">
                                <!--//Bootstrap Icons: https://icons.getbootstrap.com/ -->
                                <a class="nav-link" href="https://periapsys.com/#contact" target="_blank"
                                    rel="noopener">
                                    <span class="nav-icon">
                                        <i class="bi bi-info-circle"></i>
                                    </span>
                                    <span class="nav-link-text">Help</span> </a><!--//nav-link-->
                            </li>
                            <li class="nav-item" v-show="newVersionAvailable">
                                <a class="nav-link" href="https://github.com/periapsys/SSO/releases" target="_blank"
                                    rel="noopener">
                                    <span class="nav-icon">
                                        <i class="bi bi-exclamation-triangle"></i>
                                    </span>
                                    <span class="nav-link-text">New version available!</span> </a><!--//nav-link-->
                            </li>
                            <!--//nav-item-->
                        </ul>
                        <!--//footer-menu-->
                    </nav>
                </div>
                <!--//app-sidepanel-footer-->
            </div>
            <!--//sidepanel-inner-->
        </div>
        <!--//app-sidepanel-->
    </header>
    <TwoFactor :binaryData="modalBinaryData" />
</template>

<script>
import TwoFactor from "@/components/TwoFactor.vue";
import { getTokenInfo, getAccount } from '@/services/account.service';
import { healthCheck } from "@/services/health-check.service";
import { generate2faQrcode } from "@/services/account.service";

export default {
    components: { TwoFactor },
    data() {
        return {
            newVersionAvailable: false,
            modalBinaryData: null, // Store the fetched binary data
            modal: null,
            account: null,
        };
    },
    mounted() {
        healthCheck().then(r => {
            this.newVersionAvailable = r.data.entries.HealthCheckHandler.data.version < r.data.entries.HealthCheckHandler.data.latestVersion;
        });

        // Listen for the hidden.bs.modal event and call onModalClose method
        this.modal = new bootstrap.Modal(document.getElementById('2faModal'));

        getAccount().then(r => {
            this.account = r.data;
        });
    },
    methods: {
        onMenuClick() {
            document
                .getElementById("app-sidepanel")
                .classList.add("sidepanel-visible");
        },

        onMenuClose() {
            document
                .getElementById("app-sidepanel")
                .classList.remove("sidepanel-visible");
        },

        signout() {
            this.$router.push('/logout');
        },

        isInIdp(idp) {
            return getTokenInfo().authmethod === idp;
        },

        async on2faClick() {
            if (!this.account.twoFactorEnabled) {
                if (!confirm("Enable Two-Factor Authentication (2FA)?")) {
                    return;
                }
            }

            const blobData = await generate2faQrcode(); // Fetch the QR code as a Blob
            this.modalBinaryData = blobData; // Store the fetched Blob data
            this.modal.show();
        }
    },
};
</script>

<style scope>
.app-nav .nav-icon {
    position: absolute;
    left: 1rem;
    top: initial;
}
</style>