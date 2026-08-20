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

interface GridNode {
  x: number
  y: number
  restX: number
  restY: number
  phase: number
}

let ctx: CanvasRenderingContext2D | null = null
let nodes: GridNode[] = []
let rafId = 0
let width = 0
let height = 0
let running = false
let mouseX = -999
let mouseY = -999

const GOLD = '197, 160, 89'
const BLUE = '59, 98, 168'
const COLS = 30
const ROWS = 18
const NODE_RADIUS = 2
const CONNECT_DIST = 160

function resize() {
  if (!canvasEl.value) return
  const dpr = Math.min(window.devicePixelRatio || 1, 2)
  width = window.innerWidth
  height = window.innerHeight
  canvasEl.value.width = width * dpr
  canvasEl.value.height = height * dpr
  canvasEl.value.style.width = `${width}px`
  canvasEl.value.style.height = `${height}px`
  if (ctx) ctx.setTransform(dpr, 0, 0, dpr, 0, 0)
  spawnGrid()
}

function spawnGrid() {
  nodes = []
  const spacingX = width / (COLS - 1)
  const spacingY = height / (ROWS - 1)
  for (let row = 0; row < ROWS; row++) {
    for (let col = 0; col < COLS; col++) {
      const restX = col * spacingX
      const restY = row * spacingY
      nodes.push({
        x: restX,
        y: restY,
        restX,
        restY,
        phase: Math.random() * Math.PI * 2
      })
    }
  }
}

function draw() {
  if (!running || !ctx) return
  ctx.clearRect(0, 0, width, height)

  const t = performance.now() / 1000

  // subtle blue glow top-left
  const g1 = ctx.createRadialGradient(width * 0.15, height * 0.2, 0, width * 0.15, height * 0.2, Math.min(width, height) * 0.45)
  g1.addColorStop(0, `rgba(${BLUE}, 0.12)`)
  g1.addColorStop(1, 'rgba(0,0,0,0)')
  ctx.fillStyle = g1
  ctx.fillRect(0, 0, width, height)

  // subtle gold glow bottom-right
  const g2 = ctx.createRadialGradient(width * 0.85, height * 0.8, 0, width * 0.85, height * 0.8, Math.min(width, height) * 0.4)
  g2.addColorStop(0, `rgba(${GOLD}, 0.08)`)
  g2.addColorStop(1, 'rgba(0,0,0,0)')
  ctx.fillStyle = g2
  ctx.fillRect(0, 0, width, height)

  // animate node positions: gentle drift + mouse repulsion
  for (const n of nodes) {
    n.x = n.restX + Math.sin(t * 0.25 + n.phase) * 6
    n.y = n.restY + Math.cos(t * 0.2 + n.phase) * 4

    const dx = n.x - mouseX
    const dy = n.y - mouseY
    const dist = Math.sqrt(dx * dx + dy * dy)
    if (dist < 180 && dist > 0) {
      const push = (180 - dist) / 180 * 14
      n.x += (dx / dist) * push
      n.y += (dy / dist) * push
    }
  }

  // draw connections
  ctx.lineWidth = 0.6
  for (let i = 0; i < nodes.length; i++) {
    for (let j = i + 1; j < nodes.length; j++) {
      const a = nodes[i]
      const b = nodes[j]
      const dx = a.x - b.x
      const dy = a.y - b.y
      const dist = Math.sqrt(dx * dx + dy * dy)
      if (dist < CONNECT_DIST) {
        const alpha = (1 - dist / CONNECT_DIST) * 0.18
        ctx.strokeStyle = `rgba(${BLUE}, ${alpha})`
        ctx.beginPath()
        ctx.moveTo(a.x, a.y)
        ctx.lineTo(b.x, b.y)
        ctx.stroke()
      }
    }
  }

  // draw nodes
  for (const n of nodes) {
    const dx = n.x - mouseX
    const dy = n.y - mouseY
    const distMouse = Math.sqrt(dx * dx + dy * dy)
    const proximity = distMouse < 180 ? (180 - distMouse) / 180 : 0
    const isGold = proximity > 0.5
    const baseAlpha = 0.25 + proximity * 0.5
    const twinkle = 0.8 + 0.2 * Math.sin(t * 1.2 + n.phase)
    const alpha = baseAlpha * twinkle
    const r = NODE_RADIUS + proximity * 1.5

    ctx.beginPath()
    ctx.arc(n.x, n.y, r, 0, Math.PI * 2)
    ctx.fillStyle = isGold
      ? `rgba(${GOLD}, ${Math.min(alpha + 0.25, 1)})`
      : `rgba(${BLUE}, ${alpha})`
    ctx.fill()

    if (proximity > 0.6) {
      ctx.beginPath()
      ctx.arc(n.x, n.y, r + 4 * proximity, 0, Math.PI * 2)
      ctx.fillStyle = `rgba(${GOLD}, ${0.08 * proximity})`
      ctx.fill()
    }
  }

  rafId = requestAnimationFrame(draw)
}

function onMouseMove(e: MouseEvent) {
  mouseX = e.clientX
  mouseY = e.clientY
}

function onMouseLeave() {
  mouseX = -999
  mouseY = -999
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
  window.addEventListener('mousemove', onMouseMove)
  window.addEventListener('mouseleave', onMouseLeave)
  const mq = window.matchMedia('(prefers-reduced-motion: reduce)')
  if (!mq.matches && props.enabled) start()
  mq.addEventListener('change', (e) => {
    if (e.matches) stop()
    else if (props.enabled) start()
  })
})

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
  window.removeEventListener('mousemove', onMouseMove)
  window.removeEventListener('mouseleave', onMouseLeave)
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