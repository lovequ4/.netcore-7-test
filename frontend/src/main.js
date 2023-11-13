import { createApp } from 'vue'
import App from './App.vue'
import router from './router'
import store from './store'
import { Quasar,Notify } from 'quasar'
import '@quasar/extras/material-icons/material-icons.css'
import 'quasar/src/css/index.sass'
import axios from 'axios'

const app = createApp(App)


axios.defaults.baseURL = 'http://localhost:5030/'


app.use(router)
app.use(store)
app.use(Quasar, {
    plugins: { Notify }, 
  })   
app.mount('#app')
