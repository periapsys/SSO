import axios from 'axios';

const loadCaptcha = async (id) => {
  const url = id ? `/api/captcha?id=${id}` : '/api/captcha';
  return await axios.get(url);
};

const validateCaptcha = async (id, answer) => axios.post('api/captcha/validate', { id, answer });

export {
    loadCaptcha,
    validateCaptcha
}