import { createRouter, createWebHistory } from 'vue-router'
import Index from '../views/Index.vue'
import NavbarforAdmin from '../components/NavbarforAdmin.vue'
import DashboardProducts from '../views/DashboardProducts.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'index',
      component: Index
    },
    {
      path: '/cart',
      name: 'cart',
      // route level code-splitting
      // this generates a separate chunk (About.[hash].js) for this route
      // which is lazy-loaded when the route is visited.
      component: () => import('../views/Cart.vue')
    },
    {
      path:'/:categorySlug/:productSlug',
      name:'product',
      component: () => import('../views/Product.vue')
    },
    {
      path:'/dashboard',
      component : NavbarforAdmin,
      children:[
        {
          path:'products',
          name:'products',
          component:DashboardProducts
        }
      ]
    }
  ]
})

export default router
