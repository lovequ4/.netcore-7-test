<template>
    <div class="q-pa-md items-center justify-center" style="margin-top: 2%;">
    <q-card class="my-card" bordered style="max-width: 100%; max-width: 100ch; margin-left: 30%;">
        <q-card-section horizontal> 
            <q-img
            :src=product.image
            />
          <q-card-actions vertical class="q-flex q-justify-center">
            <div class="text-h5 q-mt-sm q-mb-xs">{{ product.name }}</div>
            <div class="text-caption">{{ product.description }}</div>
            <div><strong>Price:</strong> ${{ product.price }}</div>
            <q-input v-model="quantity" type="number" min="1" />
            <q-btn color="primary" @click="addToCart" class="q-mt-md">Add to cart</q-btn>
          </q-card-actions>
        </q-card-section>
      </q-card>
  </div>

      <!-- <q-card class="my-card" flat bordered style="max-width: 50%;">
        <q-card-section horizontal>
          <q-img class="" src=""  />
  
          <q-card-actions vertical class="q-flex q-justify-end">
            <div class="text-h5 q-mt-sm q-mb-xs">{{ product.name }}</div>
            <div class="text-caption">{{ product.description }}</div>
            <div><strong>Price:</strong> ${{ product.price }}</div>
            <q-input v-model="quantity" type="number" min="1" />
            <q-btn color="primary" @click="addToCart" class="q-mt-md">Add to cart</q-btn>
          </q-card-actions>
        </q-card-section>
      </q-card> -->
   
</template>

<script>
import axios from 'axios'

export default{
    name: 'Product',
    data(){
        return{
            product:{},
            quantity:1,
            productId: null,
        }
    },
    mounted(){
        this.getProduct()
    },
    methods:{
      async  getProduct(){   
        const categorySlug = this.$route.params.categorySlug
        const productSlug = this.$route.params.productSlug

        await  axios.get(`/api/Products/${categorySlug}/${productSlug}`)
            .then(response =>{
                this.product = response.data
                this.productId = response.data.id
            })
            .catch(error =>{
                console.log(error)
            })
           
        },
      async addToCart(){
       if(this.$store.state.isLoggedIn){
        const userId = localStorage.getItem('userId')
       
        const cartData = {
              userId : userId,
              productId : this.productId,
              quantity : this.quantity
          }
          console.log(cartData)
          await axios.post('/api/Carts/',cartData)
            .then(response => {

              this.$q.notify({
                  message: 'Add To Cart Successfully',
                  color: 'green',
                  position: 'center',
                  timeout: 500,
              });       
            })
            .catch(error => {
              console.error(error);
              this.$q.notify({
                  message: 'Add To Cart Failed , Please Try Again!',
                  color: 'negative',
                  position: 'right',
                  timeout: 500,
              });
          });
        
        }else{
          this.$q.notify({
              message: 'Must be Logged To Add Cart!',
              color: 'negative',
              position: 'right',
              timeout: 500,
          });
        } 
      },
    }
}
</script>