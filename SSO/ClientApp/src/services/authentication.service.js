import axios from 'axios';

const login = async (form) => await axios.post("/api/authentication", form);
const loginToSystem = async (form) => {
    let url = "/api/authentication/system";
    if (form.realmId !== null) {
        url += `?realmId=${form.realmId}`;
    }
    return await axios.post(url, form);
}

const forgotPassword = async (form) => await axios.post("/api/authentication/forgotpassword", form);

export {
    login,
    loginToSystem,
    forgotPassword
}