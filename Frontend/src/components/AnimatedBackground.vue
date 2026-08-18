<template>
  <div class="tecnm-animated-bg" :class="{ 'is-active': enabled }" aria-hidden="true">
    <canvas ref="canvasEl" class="tecnm-animated-bg__canvas"></canvas>
    <div class="tecnm-animated-bg__veil"></div>
  </div>
</template>

<script setup lang="ts">
import { onBeforeUnmount, onMounted, ref, watch } from 'vue'

const props = defineProps<{ enabled: boolean }>()

const canvasEl = ref<HTMLCanvasElement | null>(null)

interface Particle {
  x: number
  y: number
  r: number
  gold: boolean
  vx: number
  vy: number
  tw: number
  phase: number
}

let ctx: CanvasRenderingContext2D | null = null
let particles: Particle[] = []
let rafId = 0
let width = 0
let height = 0
let running = false

const GOLD = '197, 160, 89'
const BLUE = '59, 98, 168'

function resize() {
  if (!canvasEl.value) return
  const dpr = Math.min(window.devicePixelRatio || 1, 2)
  width = window.innerWidth
  height = window.innerHeight
  canvasEl.value.width = width * dpr
  canvasEl.value.height = height * dpr
  canvasEl.value.style.width = `${width}px`
  canvasEl.value.style.height = `${height}px`
  if (ctx) {
    ctx.setTransform(dpr, 0, 0, dpr, 0, 0)
  }
  spawn(180)
}

function spawn(count: number) {
  particles = []
  for (let i = 0; i < count; i++) {
    particles.push({
      x: Math.random() * width,
      y: Math.random() * height,
      r: 0.6 + Math.random() * 2.2,
      gold: Math.random() > 0.6,
      vx: (Math.random() - 0.5) * 0.35,
      vy: (Math.random() - 0.5) * 0.35,
      tw: 1.5 + Math.random() * 3,
      phase: Math.random() * Math.PI * 2
    })
  }
}

function draw() {
  if (!running || !ctx) return
  ctx.clearRect(0, 0, width, height)

  const t = performance.now() / 1000
  const g1 = ctx.createRadialGradient(width * 0.18, height * 0.24, 0, width * 0.18, height * 0.24, Math.min(width, height) * 0.5)
  g1.addColorStop(0, `rgba(${BLUE}, 0.22)`)
  g1.addColorStop(1, 'rgba(0,0,0,0)')
  ctx.fillStyle = g1
  ctx.fillRect(0, 0, width, height)

  const g2 = ctx.createRadialGradient(width * 0.82, height * 0.78, 0, width * 0.82, height * 0.78, Math.min(width, height) * 0.45)
  g2.addColorStop(0, `rgba(${GOLD}, 0.16)`)
  g2.addColorStop(1, 'rgba(0,0,0,0)')
  ctx.fillStyle = g2
  ctx.fillRect(0, 0, width, height)

  for (const p of particles) {
    p.x += p.vx + Math.sin(t * 0.4 + p.phase) * 0.12
    p.y += p.vy + Math.cos(t * 0.35 + p.phase) * 0.12
    if (p.x < -10) p.x = width + 10
    if (p.x > width + 10) p.x = -10
    if (p.y < -10) p.y = height + 10
    if (p.y > height + 10) p.y = -10

    const twinkle = 0.35 + 0.65 * Math.abs(Math.sin(t * p.tw + p.phase))
    ctx.beginPath()
    ctx.arc(p.x, p.y, p.r, 0, Math.PI * 2)
    ctx.fillStyle = p.gold ? `rgba(${GOLD}, ${0.75 * twinkle})` : `rgba(${BLUE}, ${0.7 * twinkle})`
    ctx.fill()
  }

  rafId = requestAnimationFrame(draw)
}

function start() {
  if (running || !canvasEl.value) return
  running = true
  resize()
  rafId = requestAnimationFrame(draw)
}

function stop() {
  running = false
  if (rafId) cancelAnimationFrame(rafId)
  rafId = 0
}

onMounted(() => {
  ctx = canvasEl.value?.getContext('2d') ?? null
  window.addEventListener('resize', resize)
  if (props.enabled) start()
})

// Toggle the animation live when the theme changes (canvas is always mounted).
watch(
  () => props.enabled,
  (value) => {
    if (value) start()
    else stop()
  },
  { flush: 'post' }
)

onBeforeUnmount(() => {
  stop()
  window.removeEventListener('resize', resize)
})
</script>

<style scoped>
.tecnm-animated-bg {
  position: fixed;
  inset: 0;
  z-index: -1;
  overflow: hidden;
  pointer-events: none;
  opacity: 0;
  visibility: hidden;
  transition: opacity 0.6s ease;
  background: radial-gradient(ellipse at 20% 10%, rgba(59, 98, 168, 0.4), transparent 55%),
    radial-gradient(ellipse at 85% 90%, rgba(197, 160, 89, 0.22), transparent 50%),
    linear-gradient(160deg, #0A1A33 0%, #0D1B33 45%, #10244D 100%);
}

.tecnm-animated-bg.is-active {
  opacity: 1;
  visibility: visible;
}

.tecnm-animated-bg__canvas {
  position: absolute;
  inset: 0;
  display: block;
}

.tecnm-animated-bg__veil {
  position: absolute;
  inset: 0;
  background: rgba(13, 27, 51, 0.35);
  pointer-events: none;
}
</style>