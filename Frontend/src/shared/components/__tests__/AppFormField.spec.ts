import { describe, it, expect } from "vitest"
import { mount } from "@vue/test-utils"
import AppFormField from "../AppFormField.vue"

describe("AppFormField.vue Component", () => {
  it("renderiza el label correctamente", () => {
    const wrapper = mount(AppFormField, {
      props: { label: "Motivo de la ausencia" }
    })
    const label = wrapper.find("label.tecnm-label")
    expect(label.exists()).toBe(true)
    expect(label.text()).toBe("Motivo de la ausencia")
  })

  it("no renderiza label si no se proporciona el prop", () => {
    const wrapper = mount(AppFormField)
    expect(wrapper.find("label").exists()).toBe(false)
  })

  it("muestra mensaje de error con rol alert si hay prop de error", () => {
    const wrapper = mount(AppFormField, {
      props: { label: "Fecha inicio", error: "La fecha es obligatoria" }
    })
    const errorSpan = wrapper.find("span.tecnm-form-error")
    expect(errorSpan.exists()).toBe(true)
    expect(errorSpan.attributes("role")).toBe("alert")
    expect(errorSpan.text()).toBe("La fecha es obligatoria")
  })

  it("no muestra mensaje de error si prop error esta vacio", () => {
    const wrapper = mount(AppFormField, {
      props: { label: "Nombre" }
    })
    expect(wrapper.find("span.tecnm-form-error").exists()).toBe(false)
  })
})
