import axios from 'axios';

const loadCaptcha = async () => await axios.get('/api/captcha');
const validateCaptcha = async (id, answer) => axios.post('api/captcha/validate', { id, answer });

export {
    loadCaptcha,
    validateCaptcha
}