<template>
    <div class="q-pa-md q-gutter-md">
      <div class="q-gutter-md row items-center justify-center">
        <router-link
        v-for="product in products"
        :key="product.id"
        v-bind:to="{ name: 'product', params: { categorySlug: product.category.slug, productSlug: product.slug } }"
        class="col-3 q-mb-md"
        style="text-decoration: none"
        >
          <q-card >
            <q-card-section>
              <q-img :src="product.image" style="height: 50ch;"/>
            </q-card-section>
            <q-card-section>
              <div class="text-h6">{{ product.name }}</div>
              <div class="text-h6">Price: ${{ product.price }}</div>
            </q-card-section>
          </q-card>
        </router-link>
      </div>
    </div>
</template>
  

<script>
import axios from 'axios';

export default {
    data() {
        return {
            products: {},
        };
    },
    mounted() {
        this.getProduct()
    },
    methods: {
      async getProduct(){
        await axios.get('/api/Products')
          .then(response=>{
            this.products = response.data
            console.log(response.data)
          })
          .catch(error =>{
            console.log(error)
          })
      },
    },
};
</script>

