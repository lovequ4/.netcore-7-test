<template>
    <q-layout view="hHh Lpr lFf" style="min-height: 40px;">
        <q-header elevated class="bg-black text-white">
            <q-toolbar>
                <a href="/" style="text-decoration: none;color: white;">
                        DEMO
                </a>
            
                <!-- Search Bar -->
               <q-toolbar style="max-width:50%">
                    <q-input dark dense standout v-model="text" input-class="text-right" class="q-ml-md">
                        <template v-slot:append>
                            <q-icon name="search" class="cursor-pointer" />
                        </template>
                    </q-input>
                </q-toolbar>
  
  
            <div class="q-gutter-md flex-grow" />
  
            <div class="q-gutter-md">
                <q-btn color="" icon="shopping_cart" label=""  @click="handleCartClick"/>
            </div>
  
            <!-- Dropdown Menu -->
            <q-btn v-if="isLoggedIn" :label= username>
                <q-menu>
                    <q-list style="min-width: 100px">
                        <q-item clickable v-close-popup>
                            <q-item-section>MyOrder</q-item-section>
                        </q-item>
  
                        <q-separator />
  
                        <q-item clickable v-close-popup>
                            <q-item-section @click="handleLogout">Logout</q-item-section>
                        </q-item>
  
                    </q-list>
                </q-menu>
            </q-btn>
  
            <q-btn v-else @click="handleLogin">Login</q-btn>

            <!-- LoginDialog -->
            <q-dialog v-model="showLoginDialog">
                <q-card style="width: 500px; max-width: 200vw;">
                    <q-card-section>
                        <div class="text-h6" align="center">Login User</div>
                        <q-input v-model="UserData.email" label="Email"></q-input>
                        <q-input v-model="UserData.password"  label="Password" type="password"></q-input>
                      
                    </q-card-section>
                    <q-card-actions align="center">
                        <q-btn label="Login" color="primary" @click="onLogin"></q-btn>
                    </q-card-actions>
                    <div style="text-align: center;margin-top: 5%;">No have Account?  
                        <a href="#" @click="heandleRegister" style="text-decoration: none;">Sign Up</a>
                    </div>
                </q-card>
            </q-dialog>


            <!-- RegisterDialog -->
            <q-dialog v-model="showRegisterDialog">
                <q-card style="width: 500px; max-width: 200vw;">
                    <q-card-section>
                        <div class="text-h6" align="center">Register User</div>
                        <q-input v-model="UserData.name" label="Name"></q-input>
                        <q-input v-model="UserData.email"  label="Email"></q-input>
                        <q-input v-model="UserData.password"  label="Password" type="password"></q-input>
                    </q-card-section>
                    <q-card-actions align="center">
                        <q-btn label="Register" color="primary" @click="onRegister"></q-btn>
                        <q-btn label="Cancel" color="negative" @click="cancelRegister"></q-btn>
                    </q-card-actions>
                </q-card>
            </q-dialog>


             <!-- Logout Dialog -->
            <q-dialog v-model="showLogoutDialog"> 
                <q-card>
                    <q-card-section>
                    <h5>Are you sure you want to Logout?</h5>
                    </q-card-section>
                    <q-card-actions align="right">
                    <q-btn label="Sure" color="negative" @click="confirmLogout"></q-btn>
                    <q-btn label="Cancel" color="primary" @click="cancelLogout"></q-btn>
                    </q-card-actions>
                </q-card> 
            </q-dialog>

            </q-toolbar>
        </q-header>

    </q-layout>
</template>
    


<script>
import { RouterLink } from 'vue-router';
import axios from 'axios'
import { jwtDecode } from "jwt-decode";

export default{
    data() {
        return{
            showLoginDialog: false,
            showRegisterDialog:false,
            showLogoutDialog:false,
            UserData:{},
            username: localStorage.getItem('username'),
            userId:"",
            role:"",
            isLoggedIn: localStorage.getItem('isLoggedIn') === 'true',
        }
    },
    methods:{
        handleCartClick(){
            if(this.$store.state.isLoggedIn){
                this.$router.push("/cart")
            }else{
                this.$q.notify({
                  message: 'Must be Logged!',
                  color: 'negative',
                  position: 'right',
                  timeout: 500,
              });
            }
            
        },
        handleLogin(){
            this.showLoginDialog = true
        },
        heandleRegister(){
            this.showRegisterDialog = true
            this.showLoginDialog = false
        },
        async onLogin(){
            const LoginData = {
                email : this.UserData.email,
                password : this.UserData.password
            }
            await axios.post('/api/Authenticate/login',LoginData)
            .then(response => {
                const token = response.data.token
                const decodedToken = jwtDecode(token);
                
                this.username = decodedToken.sub;
                this.userId = decodedToken.nameid;
                
                this.$q.notify({
                    message: 'Login Successfully',
                    color: 'green',
                    position: 'center',
                    timeout: 520,
                });

                this.showLoginDialog = false

                this.isLoggedIn = true;
                localStorage.setItem('isLoggedIn', 'true');
               
                localStorage.setItem('userId',this.userId)
                localStorage.setItem('username',this.username)
                localStorage.setItem('token',token);

                if (decodedToken.role && decodedToken.role.includes('Admin')) {
                
                    this.$router.push('/dashboard');
                }
            })
            .catch(error => {
                console.error(error);
                this.showLoginDialog = false
                this.$q.notify({
                    message: 'Login Failed',
                    color: 'negative',
                    position: 'center',
                    timeout: 500,
                });

            });
        },
        async onRegister(){
            const RegisterData = {
                name: this.UserData.name,
                password: this.UserData.password,
                email: this.UserData.email
            }
            await axios.post('/api/Authenticate/register',RegisterData)
            .then(response => {
                console.log(response.data);
                
                this.$q.notify({
                    message: 'Register Successfully',
                    color: 'success',
                    position: 'center',
                    timeout: 1000,
                });

            })
            .catch(error => {
                console.error(error);
                this.$q.notify({
                    message: 'Register Failed',
                    color: 'negative',
                    position: 'center',
                    timeout: 1000,
                });
            });
        },
        cancelRegister(){
            this.showRegisterDialog = false
        },
        handleLogout(){
            this.showLogoutDialog = true
        },
        confirmLogout(){
            localStorage.removeItem('username')
            localStorage.removeItem('userId')
            localStorage.removeItem('token');
            
            this.isLoggedIn = false;
            localStorage.setItem('isLoggedIn', 'false');
            
            this.showLogoutDialog = false
            
            this.$store.commit('setLoggedIn',false)

            this.$q.notify({
                    message: 'Logout Successfully',
                    color: 'green',
                    position: 'center',
                    timeout: 500,
                });
        }, 
        cancelLogout(){
            this.showLogoutDialog = false
        }, 
    }
}
</script>

  
<style>
.flex-grow {
  flex-grow: 1;
}

</style>
  