import axios from 'axios';

const modifyMailerSetting = async (param) => await axios.post("api/realm/mailer", param);
const deleteMailerSetting = async () => await axios.delete(`/api/realm/mailer`);
const testMailerSetting = async (param) => await axios.post(`/api/realm/mailer/test`, param);
const getMailerSetting = async () => await axios.get(`/api/realm/mailer`)

export {
    modifyMailerSetting,
    deleteMailerSetting,
    testMailerSetting,
    getMailerSetting
}