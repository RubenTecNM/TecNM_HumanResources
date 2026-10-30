import { describe, it, expect } from "vitest"
import { mount } from "@vue/test-utils"
import AppButton from "../AppButton.vue"

describe("AppButton.vue Component", () => {
  it("renderiza el contenido del slot por defecto", () => {
    const wrapper = mount(AppButton, {
      slots: {
        default: "Guardar Solicitud"
      }
    })
    expect(wrapper.text()).toContain("Guardar Solicitud")
    expect(wrapper.classes()).toContain("tecnm-btn-primary")
  })

  it("aplica la clase de variante correcta", () => {
    const wrapper = mount(AppButton, {
      props: { variant: "danger" }
    })
    expect(wrapper.classes()).toContain("tecnm-btn-danger")
  })

  it("muestra spinner y deshabilita boton cuando loading es true", () => {
    const wrapper = mount(AppButton, {
      props: { loading: true },
      slots: { default: "Cargando..." }
    })
    const spinner = wrapper.find("i.fa-spinner.fa-spin")
    expect(spinner.exists()).toBe(true)
    expect(wrapper.attributes("disabled")).toBeDefined()
  })

  it("deshabilita el boton cuando disabled es true", () => {
    const wrapper = mount(AppButton, {
      props: { disabled: true }
    })
    expect(wrapper.attributes("disabled")).toBeDefined()
  })

  it("renderiza icono personalizado si se especifica y loading es false", () => {
    const wrapper = mount(AppButton, {
      props: { icon: "fa-check", loading: false }
    })
    expect(wrapper.find("i.fa-check").exists()).toBe(true)
  })
})
